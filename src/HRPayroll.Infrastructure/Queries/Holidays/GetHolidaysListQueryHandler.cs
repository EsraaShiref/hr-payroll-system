using ErrorOr;
using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Holidays.GetHolidaysList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Holidays;

internal class GetHolidaysListQueryHandler
    : IRequestHandler<GetHolidaysListQuery, ErrorOr<List<HolidayDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetHolidaysListQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<ErrorOr<List<HolidayDto>>> Handle(GetHolidaysListQuery query, CancellationToken ct)
    {
        var holidays = _dbContext.Holidays
            .AsNoTracking()
            .Where(h => !h.IsDeleted);

        if (query.Year.HasValue)
        {
            var year = query.Year.Value;
            holidays = holidays.Where(h => h.Date.Year == year || h.IsRecurringYearly);
        }

        var result = await holidays
            .OrderByDescending(h => h.Date)
            .Select(h => new HolidayDto
            {
                Id = h.Id,
                Name = h.Name,
                Date = h.Date,
                IsRecurringYearly = h.IsRecurringYearly,
            })
            .ToListAsync(ct);

        return result;
    }
}
