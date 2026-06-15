using ErrorOr;
using HRPayroll.Application.DTOs.Payroll;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunDetail;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Payroll;

internal class GetPayrollRunDetailQueryHandler
    : IRequestHandler<GetPayrollRunDetailQuery, ErrorOr<PayrollRunDetailDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPayrollRunDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<PayrollRunDetailDto>> Handle(
        GetPayrollRunDetailQuery query, CancellationToken ct)
    {
        var empQuery = from d in _dbContext.PayrollRunDetails
                       join e in _dbContext.Employees on d.EmployeeId equals e.Id
                       where d.PayrollRunId == query.PayrollRunId
                          && d.EmployeeId == query.EmployeeId
                          && !d.IsDeleted
                       select new { Detail = d, Employee = e };

        var result = await empQuery.FirstOrDefaultAsync(ct);

        if (result == null)
            return Error.NotFound("PayrollRunDetail.NotFound", "Detail not found for this employee in the specified run.");

        var dto = MapDetail(result.Detail, result.Employee);
        return dto;
    }

    private static PayrollRunDetailDto MapDetail(
        Domain.Entities.PayrollRunDetail detail,
        Domain.Entities.Employee employee)
    {
        return new PayrollRunDetailDto
        {
            EmployeeId = detail.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            EmployeeCode = employee.EmployeeCode?.Value ?? "",
            Status = detail.Status.ToString(),
            SkipReason = detail.SkipReason?.ToString(),
            FailureMessage = detail.FailureMessage,
            TotalScheduledDays = detail.TotalScheduledDays,
            TotalPresentDays = detail.TotalPresentDays,
            TotalAbsentDays = detail.TotalAbsentDays,
            TotalLeaveDays = detail.TotalLeaveDays,
            TotalOvertimeMinutes = detail.TotalOvertimeMinutes,
            LateOccurrenceCount = detail.LateOccurrenceCount,
            LatePenaltyUnits = detail.LatePenaltyUnits,
            BaseSalary = detail.BaseSalary,
            TotalAllowances = detail.TotalAllowances,
            OvertimePay = detail.OvertimePay,
            GrossPay = detail.GrossPay,
            LeaveDeduction = detail.LeaveDeduction,
            LatePenaltyDeduction = detail.LatePenaltyDeduction,
            SocialInsuranceEmployeeShare = detail.SocialInsuranceEmployeeShare,
            TaxableIncome = detail.TaxableIncome,
            TaxAmount = detail.TaxAmount,
            TotalDeductions = detail.TotalDeductions,
            NetPay = detail.NetPay,
        };
    }
}
