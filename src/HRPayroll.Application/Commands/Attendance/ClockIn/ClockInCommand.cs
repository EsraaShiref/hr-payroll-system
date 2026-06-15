using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.ClockIn;

public sealed record ClockInCommand(Guid EmployeeId, DateOnly Date, TimeOnly Time) : IRequest<ErrorOr<Guid>>;
