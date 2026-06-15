using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.Cancel;

public sealed record CancelLeaveRequestCommand(Guid LeaveRequestId) : IRequest<ErrorOr<Success>>;
