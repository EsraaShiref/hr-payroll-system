using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.Approve;

public class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, ErrorOr<Success>>
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveLeaveRequestCommandHandler(ILeaveRequestRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(ApproveLeaveRequestCommand command, CancellationToken ct)
    {
        var leaveRequest = await _repository.GetByIdAsync(command.LeaveRequestId, ct);
        if (leaveRequest == null)
            return Error.NotFound("LeaveRequest.NotFound", "Leave request not found.");

        leaveRequest.Approve(command.ApprovedBy);
        leaveRequest.ClearDomainEvents();

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
