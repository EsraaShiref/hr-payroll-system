using ErrorOr;
using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Shifts.GetShiftsList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Shifts;

internal class GetShiftsListQueryHandler
    : IRequestHandler<GetShiftsListQuery, ErrorOr<List<ShiftDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetShiftsListQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<ErrorOr<List<ShiftDto>>> Handle(GetShiftsListQuery query, CancellationToken ct)
    {
        var shifts = await _dbContext.Shifts
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new ShiftDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                StartTime = s.StartTime.ToString("HH:mm"),
                EndTime = s.EndTime.ToString("HH:mm"),
                GracePeriodMinutes = s.GracePeriodMinutes,
                LateThresholdMinutes = s.LateThresholdMinutes,
                EarlyDepartureThresholdMinutes = s.EarlyDepartureThresholdMinutes,
                OvertimeThresholdMinutes = s.OvertimeThresholdMinutes,
                MinimumWorkMinutesForPresence = s.MinimumWorkMinutesForPresence,
                MaxBreakMinutes = s.MaxBreakMinutes,
                WorkingDays = (int)s.WorkingDays,
            })
            .ToListAsync(ct);

        return shifts;
    }
}
