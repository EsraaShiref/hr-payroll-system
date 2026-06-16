using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.SubmitMy;

public sealed record SubmitMyLeaveRequestCommand(
    string LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Reason) : IRequest<ErrorOr<Guid>>;
