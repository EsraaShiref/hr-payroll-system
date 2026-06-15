using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.ClockOut;

public sealed record ClockOutCommand(Guid EmployeeId, DateOnly Date, TimeOnly Time, int BreakDurationMinutes = 0) : IRequest<ErrorOr<Success>>;
