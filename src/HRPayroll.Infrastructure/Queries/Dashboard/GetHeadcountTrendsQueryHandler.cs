using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Dashboard.GetHeadcountTrends;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Dashboard;

internal sealed class GetHeadcountTrendsQueryHandler
    : IRequestHandler<GetHeadcountTrendsQuery, ErrorOr<HeadcountTrendDto>>
{
    private readonly IApplicationDbContext _db;

    public GetHeadcountTrendsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<HeadcountTrendDto>> Handle(
        GetHeadcountTrendsQuery query, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-(query.Months - 1));

        var allEmployees = await _db.Employees
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Select(e => new
            {
                e.HireDate,
                e.TerminationDate,
                e.EmploymentStatus,
            })
            .ToListAsync(ct);

        var months = new List<MonthlyHeadcountDto>();
        int? previousCount = null;

        for (var m = startMonth; m <= today; m = m.AddMonths(1))
        {
            var monthEnd = new DateOnly(m.Year, m.Month, DateTime.DaysInMonth(m.Year, m.Month));

            var count = allEmployees.Count(e =>
                e.HireDate <= monthEnd
                && (!e.TerminationDate.HasValue || e.TerminationDate.Value > monthEnd));

            var change = previousCount.HasValue ? count - previousCount.Value : 0;
            months.Add(new MonthlyHeadcountDto
            {
                Label = m.ToString("MMM yyyy"),
                Year = m.Year,
                Month = m.Month,
                Count = count,
                ChangeFromPrevious = change,
            });
            previousCount = count;
        }

        return new HeadcountTrendDto
        {
            Months = months,
            CurrentHeadcount = previousCount ?? 0,
        };
    }
}
