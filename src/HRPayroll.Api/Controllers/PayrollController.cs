using HRPayroll.Application.Commands.Payroll.ApprovePayrollRun;
using HRPayroll.Application.Commands.Payroll.FinalizePayrollRun;
using HRPayroll.Application.Commands.Payroll.PatchRunPayroll;
using HRPayroll.Application.Commands.Payroll.RejectPayrollRun;
using HRPayroll.Application.Commands.Payroll.RunPayroll;
using HRPayroll.Application.Common.Security;
using HRPayroll.Application.DTOs.Payroll;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunDetail;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunsList;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunStatus;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRPayroll.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Api.Controllers;

[Authorize(Policy = PayrollPolicies.PayrollManage)]
public class PayrollController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IPaymentFileExportService _paymentFileExport;
    private readonly IPayslipGeneratorService _payslipGenerator;
    private readonly IApplicationDbContext _dbContext;

    public PayrollController(
        IMediator mediator,
        IPaymentFileExportService paymentFileExport,
        IPayslipGeneratorService payslipGenerator,
        IApplicationDbContext dbContext)
    {
        _mediator = mediator;
        _paymentFileExport = paymentFileExport;
        _payslipGenerator = payslipGenerator;
        _dbContext = dbContext;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunPayrollRequest request, CancellationToken ct)
    {
        var command = new RunPayrollCommand(request.Year, request.Month, request.Notes);
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetPayrollRunsListQuery(page, pageSize), ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollRunStatusQuery(id), ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollRunSummaryQuery(id), ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}/details/{employeeId:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, Guid employeeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollRunDetailQuery(id, employeeId), ct);
        return OkOrError(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApprovePayrollRunCommand(id), ct);
        return OkOrError(result);
    }

    [HttpPost("{id:guid}/finalize")]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new FinalizePayrollRunCommand(id), ct);
        return OkOrError(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectPayrollRunRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectPayrollRunCommand(id, request.Reason), ct);
        return OkOrError(result);
    }

    [HttpPost("patch")]
    public async Task<IActionResult> Patch([FromBody] PatchRunPayrollRequest request, CancellationToken ct)
    {
        var command = new PatchRunPayrollCommand(request.OriginalRunId, request.EmployeeIds);
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}/export/csv")]
    public async Task<IActionResult> ExportCsv(Guid id, CancellationToken ct)
    {
        var details = await _dbContext.PayrollRunDetails
            .AsNoTracking()
            .Where(d => d.PayrollRunId == id && !d.IsDeleted
                && d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated)
            .Include(d => d.Employee)
            .ToListAsync(ct);

        if (details.Count == 0)
            return NotFound("No calculated payroll details found for this run.");

        var csvBytes = await _paymentFileExport.GenerateBankTransferCsvAsync(details, ct);
        return File(csvBytes, "text/csv", $"payroll-export-{id}.csv");
    }

    [HttpGet("{id:guid}/export/payslips")]
    public async Task<IActionResult> ExportPayslips(Guid id, CancellationToken ct)
    {
        var run = await _dbContext.PayrollRuns.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null) return NotFound("Payroll run not found.");

        var details = await _dbContext.PayrollRunDetails
            .AsNoTracking()
            .Where(d => d.PayrollRunId == id && !d.IsDeleted
                && d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated)
            .Include(d => d.Employee).ThenInclude(e => e.Department)
            .Include(d => d.Employee).ThenInclude(e => e.Position)
            .Include(d => d.Employee).ThenInclude(e => e.Contracts.Where(c => c.Status == Domain.Enums.ContractStatus.Active))
            .ToListAsync(ct);

        if (details.Count == 0)
            return NotFound("No calculated payroll details found for this run.");

        var versionIds = details.Select(d => d.ContractVersionId).Distinct().ToList();
        var allowances = await _dbContext.ContractVersions
            .AsNoTracking()
            .Where(v => versionIds.Contains(v.Id))
            .SelectMany(v => v.AllowanceAssignments)
            .Include(a => a.Allowance)
            .ToListAsync(ct);

        var allowanceLookup = allowances
            .GroupBy(a => a.ContractVersionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var periodLabel = $"{new DateTime(run.Year, run.Month, 1):MMMM yyyy}";

        // TODO: Generate ZIP archive; for now single PDF of first employee
        var d = details.First();
        var emp = d.Employee;
        var activeContract = emp.Contracts?.FirstOrDefault();
        var empAllowances = allowanceLookup.GetValueOrDefault(d.ContractVersionId) ?? new();

        var earningsList = new List<PayslipEarningLine>
        {
            new("Base Salary", d.BaseSalary),
        };
        foreach (var aa in empAllowances.OrderBy(a => a.Allowance?.Name))
        {
            earningsList.Add(new(aa.Allowance?.Name ?? "Allowance", aa.ComputeValue(Money.Create(d.BaseSalary, "USD")).Amount));
        }
        if (d.OvertimePay > 0)
            earningsList.Add(new("Overtime Pay", d.OvertimePay));

        var deductionsList = new List<PayslipDeductionLine>
        {
            new("Social Insurance (Employee Share)", d.SocialInsuranceEmployeeShare),
            new("Income Tax", d.TaxAmount),
        };
        if (d.LeaveDeduction > 0)
            deductionsList.Add(new("Leave Deduction", d.LeaveDeduction));
        if (d.LatePenaltyDeduction > 0)
            deductionsList.Add(new("Late Penalty", d.LatePenaltyDeduction));

        var payslipData = new PayslipData
        {
            CompanyName = "HRPayroll Corp",
            PeriodLabel = periodLabel,
            EmployeeId = d.EmployeeId,
            EmployeeName = $"{emp.FirstName} {emp.LastName}",
            EmployeeCode = emp.EmployeeCode?.Value ?? "",
            Department = emp.Department?.Name ?? "",
            Position = emp.Position?.Title ?? "",
            ContractType = activeContract?.ContractType.ToString() ?? "",
            PresentDays = d.TotalPresentDays,
            AbsentDays = d.TotalAbsentDays,
            LeaveDays = d.TotalLeaveDays,
            LateOccurrences = d.LateOccurrenceCount,
            OvertimeHours = Math.Round(d.TotalOvertimeMinutes / 60m, 1),
            Earnings = earningsList,
            GrossPay = d.GrossPay,
            Deductions = deductionsList,
            TotalDeductions = d.TotalDeductions,
            NetPay = d.NetPay,
            CalculatedAt = run.CompletedAt ?? run.StartedAt ?? DateTime.UtcNow,
            PayrollRunId = id,
        };

        var pdfBytes = await _payslipGenerator.GeneratePayslipPdfAsync(payslipData, ct);
        return File(pdfBytes, "application/pdf", $"payslip-{d.EmployeeId}.pdf");
    }
}

public record RunPayrollRequest(int Year, int Month, string? Notes);

public record RejectPayrollRunRequest(string Reason);

public record PatchRunPayrollRequest(Guid OriginalRunId, List<Guid> EmployeeIds);
