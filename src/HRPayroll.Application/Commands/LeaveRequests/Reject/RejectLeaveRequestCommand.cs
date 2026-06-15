using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.Reject;

public sealed record RejectLeaveRequestCommand(Guid LeaveRequestId, Guid RejectedBy, string Reason) : IRequest<ErrorOr<Success>>;
