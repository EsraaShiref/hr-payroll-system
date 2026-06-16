using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.DisputeAttendance;

public sealed record DisputeAttendanceSummaryCommand(
    Guid SummaryId,
    TimeOnly? ClaimedPunchIn,
    TimeOnly? ClaimedPunchOut,
    string Reason) : IRequest<ErrorOr<Success>>;
