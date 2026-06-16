using ErrorOr;
using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Shifts.GetShiftById;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Shifts;

internal class GetShiftByIdQueryHandler
    : IRequestHandler<GetShiftByIdQuery, ErrorOr<ShiftDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetShiftByIdQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<ErrorOr<ShiftDto>> Handle(GetShiftByIdQuery query, CancellationToken ct)
    {
        var shift = await _dbContext.Shifts
            .AsNoTracking()
            .Where(s => s.Id == query.Id && !s.IsDeleted)
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
            .FirstOrDefaultAsync(ct);

        if (shift == null)
            return Error.NotFound("Shift.NotFound", "Shift not found.");

        return shift;
    }
}
