using ErrorOr;
using HRPayroll.Application.DTOs.Payroll;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunsList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Payroll;

internal class GetPayrollRunsListQueryHandler
    : IRequestHandler<GetPayrollRunsListQuery, ErrorOr<PaginatedPayrollRunsDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPayrollRunsListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<PaginatedPayrollRunsDto>> Handle(
        GetPayrollRunsListQuery query, CancellationToken ct)
    {
        var baseQuery = _dbContext.PayrollRuns
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new PayrollRunListItemDto
            {
                Id = r.Id,
                Year = r.Year,
                Month = r.Month,
                Status = r.Status,
                StartedAt = r.StartedAt,
                CompletedAt = r.CompletedAt,
                ProcessedBy = r.ProcessedBy,
                TotalEmployees = r.Details.Count,
                TotalNetPay = r.Details
                    .Where(d => !d.IsDeleted && d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated)
                    .Sum(d => d.NetPay),
            })
            .ToListAsync(ct);

        return new PaginatedPayrollRunsDto(items, totalCount, query.Page, query.PageSize);
    }
}
