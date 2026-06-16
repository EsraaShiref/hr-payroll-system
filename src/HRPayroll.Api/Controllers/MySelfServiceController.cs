using HRPayroll.Application.Commands.Attendance.DisputeAttendance;
using HRPayroll.Application.Commands.LeaveRequests.SubmitMy;
using HRPayroll.Application.Commands.RequestEmailChange;
using HRPayroll.Application.Commands.UpdateMyInfo;
using HRPayroll.Application.DTOs.Attendance;
using HRPayroll.Domain.Enums;
using HRPayroll.Application.DTOs.Payroll;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.LeaveRequests.GetLeaveBalances;
using HRPayroll.Application.Queries.LeaveRequests.GetMyLeaveRequests;
using HRPayroll.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Api.Controllers;

[Authorize]
[Route("api/my")]
public class MySelfServiceController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPayslipGeneratorService _payslipGenerator;

    public MySelfServiceController(
        IMediator mediator,
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IPayslipGeneratorService payslipGenerator)
    {
        _mediator = mediator;
        _db = db;
        _currentUser = currentUser;
        _payslipGenerator = payslipGenerator;
    }

    [HttpGet("payslips")]
    public async Task<IActionResult> GetMyPayslips(CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null) return Forbid();

        var runs = await _db.PayrollRunDetails
            .AsNoTracking()
            .Where(d => d.EmployeeId == empId.Value && !d.IsDeleted
                && d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated)
            .Join(_db.PayrollRuns.Where(r => r.Status == Domain.Enums.PayrollRunStatus.Finalized),
                d => d.PayrollRunId, r => r.Id,
                (d, r) => new
                {
                    RunId = r.Id,
                    r.Year,
                    r.Month,
                    r.CompletedAt,
                    d.NetPay,
                    d.BaseSalary,
                    d.GrossPay,
                    d.TotalDeductions,
                })
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .ToListAsync(ct);

        var result = runs.Select(r => new
        {
            r.RunId,
            r.Year,
            r.Month,
            Period = new DateTime(r.Year, r.Month, 1).ToString("MMMM yyyy"),
            r.CompletedAt,
            r.NetPay,
            r.GrossPay,
            r.TotalDeductions,
            r.BaseSalary,
        }).ToList();

        return Ok(result);
    }

    [HttpGet("payslips/{runId:guid}/pdf")]
    public async Task<IActionResult> GetMyPayslipPdf(Guid runId, CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null) return Forbid();

        var detail = await _db.PayrollRunDetails
            .AsNoTracking()
            .Include(d => d.Employee).ThenInclude(e => e.Department)
            .Include(d => d.Employee).ThenInclude(e => e.Position)
            .Include(d => d.Employee).ThenInclude(e => e.Contracts.Where(c => c.Status == Domain.Enums.ContractStatus.Active))
            .FirstOrDefaultAsync(d => d.PayrollRunId == runId && d.EmployeeId == empId.Value
                && !d.IsDeleted && d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated, ct);

        if (detail is null) return NotFound();

        var run = await _db.PayrollRuns.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == runId, ct);
        if (run is null) return NotFound();

        var allowances = await _db.ContractVersions
            .AsNoTracking()
            .Where(v => v.Id == detail.ContractVersionId)
            .SelectMany(v => v.AllowanceAssignments)
            .Include(a => a.Allowance)
            .ToListAsync(ct);

        var emp = detail.Employee;
        var activeContract = emp.Contracts?.FirstOrDefault();

        var earningsList = new List<PayslipEarningLine> { new("Base Salary", detail.BaseSalary) };
        foreach (var aa in allowances.OrderBy(a => a.Allowance?.Name))
            earningsList.Add(new(aa.Allowance?.Name ?? "Allowance",
                aa.ComputeValue(Money.Create(detail.BaseSalary, "USD")).Amount));
        if (detail.OvertimePay > 0)
            earningsList.Add(new("Overtime Pay", detail.OvertimePay));

        var deductionsList = new List<PayslipDeductionLine>
        {
            new("Social Insurance (Employee Share)", detail.SocialInsuranceEmployeeShare),
            new("Income Tax", detail.TaxAmount),
        };
        if (detail.LeaveDeduction > 0) deductionsList.Add(new("Leave Deduction", detail.LeaveDeduction));
        if (detail.LatePenaltyDeduction > 0) deductionsList.Add(new("Late Penalty", detail.LatePenaltyDeduction));

        var data = new PayslipData
        {
            CompanyName = "HRPayroll Corp",
            PeriodLabel = new DateTime(run.Year, run.Month, 1).ToString("MMMM yyyy"),
            EmployeeId = detail.EmployeeId,
            EmployeeName = $"{emp.FirstName} {emp.LastName}",
            EmployeeCode = emp.EmployeeCode?.Value ?? "",
            Department = emp.Department?.Name ?? "",
            Position = emp.Position?.Title ?? "",
            ContractType = activeContract?.ContractType.ToString() ?? "",
            PresentDays = detail.TotalPresentDays,
            AbsentDays = detail.TotalAbsentDays,
            LeaveDays = detail.TotalLeaveDays,
            LateOccurrences = detail.LateOccurrenceCount,
            OvertimeHours = Math.Round(detail.TotalOvertimeMinutes / 60m, 1),
            Earnings = earningsList,
            GrossPay = detail.GrossPay,
            Deductions = deductionsList,
            TotalDeductions = detail.TotalDeductions,
            NetPay = detail.NetPay,
            CalculatedAt = run.CompletedAt ?? run.StartedAt ?? DateTime.UtcNow,
            PayrollRunId = runId,
        };

        var pdfBytes = await _payslipGenerator.GeneratePayslipPdfAsync(data, ct);
        return File(pdfBytes, "application/pdf", $"payslip-{detail.EmployeeId}.pdf");
    }

    [HttpPost("leave")]
    public async Task<IActionResult> SubmitLeave(
        [FromBody] SubmitMyLeaveRequestCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpGet("leave")]
    public async Task<IActionResult> GetMyLeaveRequests(
        [FromQuery] int? year, [FromQuery] string? status, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyLeaveRequestsQuery(year, status), ct);
        return OkOrError(result);
    }

    [HttpGet("leave/balances")]
    public async Task<IActionResult> GetMyLeaveBalances(CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null) return Forbid();
        var result = await _mediator.Send(new GetLeaveBalancesQuery(empId.Value), ct);
        return OkOrError(result);
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetMyAttendance(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null) return Forbid();

        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var summaries = await _db.AttendanceDailySummaries
            .AsNoTracking()
            .Where(s => s.EmployeeId == empId.Value && s.Date >= start && s.Date <= end && !s.IsDeleted)
            .OrderBy(s => s.Date)
            .ToListAsync(ct);

        var days = summaries.Select(s => new AttendanceViewerItemDto
        {
            Id = s.Id,
            Date = s.Date,
            Status = s.Status.ToString(),
            FirstPunchIn = s.FirstPunchIn?.ToString("HH:mm"),
            LastPunchOut = s.LastPunchOut?.ToString("HH:mm"),
            NetWorkedMinutes = s.NetWorkedMinutes,
            LateMinutes = s.LateMinutes,
            EarlyDepartureMinutes = s.EarlyDepartureMinutes,
            OvertimeMinutes = s.OvertimeMinutes,
            IsOnLeave = s.IsOnLeave,
            IsHoliday = s.IsHoliday,
            Notes = s.Notes,
        }).ToList();

        var presentCount = days.Count(d => d.Status is "OnTime" or "Late" or "EarlyDeparture" or "PendingReview");

        return Ok(new AttendanceViewerResult
        {
            EmployeeId = empId.Value,
            EmployeeName = "",
            Year = year,
            Month = month,
            Days = days,
            Summary = new AttendanceViewerSummaryDto
            {
                TotalPresentDays = presentCount,
                TotalLateOccurrences = days.Count(d => d.LateMinutes > 0),
                TotalAbsentDays = days.Count(d => d.Status == "AbsentUnexcused"),
                TotalLeaveDays = days.Count(d => d.IsOnLeave),
                TotalHolidayDays = days.Count(d => d.IsHoliday),
                TotalOvertimeHours = Math.Round(days.Sum(d => d.OvertimeMinutes) / 60m, 1),
                TotalWorkedMinutes = days.Sum(d => d.NetWorkedMinutes),
            },
        });
    }

    [HttpPost("attendance/dispute")]
    public async Task<IActionResult> DisputeAttendance(
        [FromBody] DisputeAttendanceSummaryCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateMyInfoCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpPost("profile/email")]
    public async Task<IActionResult> RequestEmailChange(
        [FromBody] RequestEmailChangeCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }
}
