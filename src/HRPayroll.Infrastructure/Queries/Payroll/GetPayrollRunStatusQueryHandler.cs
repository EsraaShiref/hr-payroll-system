using ErrorOr;
using HRPayroll.Application.DTOs.Payroll;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Payroll;

internal class GetPayrollRunStatusQueryHandler
    : IRequestHandler<GetPayrollRunStatusQuery, ErrorOr<PayrollRunStatusDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPayrollRunStatusQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<PayrollRunStatusDto>> Handle(
        GetPayrollRunStatusQuery query, CancellationToken ct)
    {
        var status = await _dbContext.PayrollRuns
            .AsNoTracking()
            .Where(r => r.Id == query.PayrollRunId && !r.IsDeleted)
            .Select(r => new PayrollRunStatusDto
            {
                Id = r.Id,
                Year = r.Year,
                Month = r.Month,
                Status = r.Status,
                StartedAt = r.StartedAt,
                CompletedAt = r.CompletedAt,
                TotalEmployees = r.Details.Count(d => !d.IsDeleted),
                CalculatedCount = r.Details.Count(d =>
                    !d.IsDeleted && d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated),
                SkippedCount = r.Details.Count(d =>
                    !d.IsDeleted && d.Status == Domain.Enums.PayrollRunDetailStatus.Skipped),
                FailedCount = r.Details.Count(d =>
                    !d.IsDeleted && d.Status == Domain.Enums.PayrollRunDetailStatus.Failed),
            })
            .FirstOrDefaultAsync(ct);

        if (status == null)
            return Error.NotFound("PayrollRun.NotFound", "Payroll run not found.");

        return status;
    }
}
