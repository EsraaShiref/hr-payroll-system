using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.Approve;

public sealed record ApproveLeaveRequestCommand(Guid LeaveRequestId, Guid ApprovedBy) : IRequest<ErrorOr<Success>>;
