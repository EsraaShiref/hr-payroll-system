using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.Submit;

public class SubmitLeaveRequestCommandHandler : IRequestHandler<SubmitLeaveRequestCommand, ErrorOr<Guid>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitLeaveRequestCommandHandler(
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        IUnitOfWork unitOfWork)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(SubmitLeaveRequestCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<LeaveType>(command.LeaveType, true, out var leaveType))
            return Error.Validation("LeaveType.Invalid", $"Invalid leave type: {command.LeaveType}");

        var leaveRequest = new LeaveRequest(command.EmployeeId, leaveType, command.StartDate, command.EndDate, command.Reason);

        var balance = await _leaveBalanceRepository.GetByEmployeeAndTypeYearAsync(
            command.EmployeeId, leaveType, command.StartDate.Year, ct);

        if (balance != null)
        {
            balance.Deduct(leaveRequest.TotalDays);
        }

        _leaveRequestRepository.Add(leaveRequest);

        await _unitOfWork.SaveChangesAsync(ct);
        return leaveRequest.Id;
    }
}
