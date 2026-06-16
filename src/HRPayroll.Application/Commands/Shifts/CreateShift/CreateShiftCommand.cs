using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Shifts.CreateShift;

public sealed record CreateShiftCommand(
    string Name,
    string? Description,
    string StartTime,
    string EndTime,
    int GracePeriodMinutes,
    int LateThresholdMinutes,
    int EarlyDepartureThresholdMinutes,
    int OvertimeThresholdMinutes,
    int MinimumWorkMinutesForPresence,
    int MaxBreakMinutes,
    int WorkingDays) : IRequest<ErrorOr<Guid>>;
