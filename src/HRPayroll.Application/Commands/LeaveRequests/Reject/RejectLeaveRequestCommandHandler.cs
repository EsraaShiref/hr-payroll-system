using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.Reject;

public class RejectLeaveRequestCommandHandler : IRequestHandler<RejectLeaveRequestCommand, ErrorOr<Success>>
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectLeaveRequestCommandHandler(ILeaveRequestRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(RejectLeaveRequestCommand command, CancellationToken ct)
    {
        var leaveRequest = await _repository.GetByIdAsync(command.LeaveRequestId, ct);
        if (leaveRequest == null)
            return Error.NotFound("LeaveRequest.NotFound", "Leave request not found.");

        leaveRequest.Reject(command.RejectedBy, command.Reason);

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
