using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.ResolveOrphanPunches;

public sealed record ResolveOrphanPunchesCommand(
    Guid PunchId,
    DateOnly TargetDate) : IRequest<ErrorOr<Success>>;
