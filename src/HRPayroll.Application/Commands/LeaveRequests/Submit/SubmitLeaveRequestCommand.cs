using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.Submit;

public sealed record SubmitLeaveRequestCommand(
    Guid EmployeeId,
    string LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Reason) : IRequest<ErrorOr<Guid>>;
