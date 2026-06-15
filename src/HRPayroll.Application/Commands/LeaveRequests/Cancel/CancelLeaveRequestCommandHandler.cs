using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.Cancel;

public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, ErrorOr<Success>>
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelLeaveRequestCommandHandler(ILeaveRequestRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(CancelLeaveRequestCommand command, CancellationToken ct)
    {
        var leaveRequest = await _repository.GetByIdAsync(command.LeaveRequestId, ct);
        if (leaveRequest == null)
            return Error.NotFound("LeaveRequest.NotFound", "Leave request not found.");

        leaveRequest.Cancel();
        leaveRequest.ClearDomainEvents();

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
