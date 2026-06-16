using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Dashboard.GetMonthlyPayrollBudgetSummary;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Dashboard;

internal sealed class GetMonthlyPayrollBudgetSummaryQueryHandler
    : IRequestHandler<GetMonthlyPayrollBudgetSummaryQuery, ErrorOr<PayrollBudgetSummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetMonthlyPayrollBudgetSummaryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<PayrollBudgetSummaryDto>> Handle(
        GetMonthlyPayrollBudgetSummaryQuery query, CancellationToken ct)
    {
        // Projected gross pay: sum of BaseSalary from active ContractVersion with latest version per employee
        var projected = await _db.Employees
            .AsNoTracking()
            .Where(e => e.EmploymentStatus == Domain.Enums.EmploymentStatus.Active && !e.IsDeleted)
            .SelectMany(e => e.Contracts.Where(c => c.Status == Domain.Enums.ContractStatus.Active && !c.IsDeleted))
            .SelectMany(c => c.Versions)
            .GroupBy(v => v.Contract.EmployeeId)
            .Select(g => g.OrderByDescending(v => v.EffectiveFrom).Select(v => v.BaseSalary.Amount).FirstOrDefault())
            .SumAsync(ct);

        // Actual: finalized payroll run for the given month
        var actual = await _db.PayrollRuns
            .AsNoTracking()
            .Where(r => r.Year == query.Year && r.Month == query.Month
                && r.Status == Domain.Enums.PayrollRunStatus.Finalized)
            .SelectMany(r => r.Details.Where(d => d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalNetPay = g.Sum(d => d.NetPay),
                TotalGrossPay = g.Sum(d => d.GrossPay),
                TotalDeductions = g.Sum(d => d.TotalDeductions),
                EmployeeCount = g.Count(),
            })
            .FirstOrDefaultAsync(ct);

        // Department breakdown for actuals
        var deptBreakdown = await _db.PayrollRuns
            .AsNoTracking()
            .Where(r => r.Year == query.Year && r.Month == query.Month
                && r.Status == Domain.Enums.PayrollRunStatus.Finalized)
            .SelectMany(r => r.Details.Where(d => d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated))
            .Include(d => d.Employee).ThenInclude(e => e.Department)
            .GroupBy(d => d.Employee.Department.Name)
            .Select(g => new DepartmentBudgetDto
            {
                DepartmentName = g.Key,
                ActualNetPay = g.Sum(d => d.NetPay),
                EmployeeCount = g.Count(),
                ProjectedBaseSalaries = 0, // filled below if needed
            })
            .ToListAsync(ct);

        return new PayrollBudgetSummaryDto
        {
            Year = query.Year,
            Month = query.Month,
            ProjectedGrossPay = projected,
            ActualNetPay = actual?.TotalNetPay ?? 0,
            ActualGrossPay = actual?.TotalGrossPay ?? 0,
            ActualTotalDeductions = actual?.TotalDeductions ?? 0,
            ActualEmployeeCount = actual?.EmployeeCount ?? 0,
            DepartmentBreakdown = deptBreakdown,
        };
    }
}
