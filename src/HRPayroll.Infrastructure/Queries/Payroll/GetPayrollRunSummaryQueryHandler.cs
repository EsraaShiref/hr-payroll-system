using ErrorOr;
using HRPayroll.Application.DTOs.Payroll;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunSummary;
using HRPayroll.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Payroll;

internal class GetPayrollRunSummaryQueryHandler
    : IRequestHandler<GetPayrollRunSummaryQuery, ErrorOr<PayrollRunSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPayrollRunSummaryQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<PayrollRunSummaryDto>> Handle(
        GetPayrollRunSummaryQuery query, CancellationToken ct)
    {
        var run = await _dbContext.PayrollRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.PayrollRunId && !r.IsDeleted, ct);

        if (run == null)
            return Error.NotFound("PayrollRun.NotFound", "Payroll run not found.");

        var details = await _dbContext.PayrollRunDetails
            .AsNoTracking()
            .Where(d => d.PayrollRunId == query.PayrollRunId && !d.IsDeleted)
            .ToListAsync(ct);

        var calculated = details.Where(d => d.Status == PayrollRunDetailStatus.Calculated).ToList();

        return new PayrollRunSummaryDto
        {
            Id = run.Id,
            Year = run.Year,
            Month = run.Month,
            Status = run.Status,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt,
            ProcessedBy = run.ProcessedBy,
            TotalGrossPay = calculated.Sum(d => d.GrossPay),
            TotalDeductions = calculated.Sum(d => d.TotalDeductions),
            TotalNetPay = calculated.Sum(d => d.NetPay),
            TotalEmployees = details.Count,
            CalculatedCount = calculated.Count,
            SkippedCount = details.Count(d => d.Status == PayrollRunDetailStatus.Skipped),
            FailedCount = details.Count(d => d.Status == PayrollRunDetailStatus.Failed),
            SkippedEmployees = details
                .Where(d => d.Status == PayrollRunDetailStatus.Skipped)
                .Select(d => new SkippedEmployeeInfo(d.EmployeeId, "", d.SkipReason?.ToString() ?? ""))
                .ToList(),
            FailedEmployees = details
                .Where(d => d.Status == PayrollRunDetailStatus.Failed)
                .Select(d => new FailedEmployeeInfo(d.EmployeeId, "", d.FailureMessage ?? ""))
                .ToList(),
        };
    }
}
