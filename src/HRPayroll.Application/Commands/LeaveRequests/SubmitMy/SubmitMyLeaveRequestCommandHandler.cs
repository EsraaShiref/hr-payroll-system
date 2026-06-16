using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using MediatR;

namespace HRPayroll.Application.Commands.LeaveRequests.SubmitMy;

public class SubmitMyLeaveRequestCommandHandler
    : IRequestHandler<SubmitMyLeaveRequestCommand, ErrorOr<Guid>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitMyLeaveRequestCommandHandler(
        ICurrentUserService currentUser,
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _leaveRequestRepository = leaveRequestRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(SubmitMyLeaveRequestCommand command, CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null)
            return Error.Unauthorized("User.NoEmployee", "Current user has no linked employee record.");

        if (!Enum.TryParse<LeaveType>(command.LeaveType, true, out var leaveType))
            return Error.Validation("LeaveType.Invalid", $"Invalid leave type: {command.LeaveType}");

        var leaveRequest = new LeaveRequest(empId.Value, leaveType, command.StartDate, command.EndDate, command.Reason);

        var balance = await _leaveBalanceRepository.GetByEmployeeAndTypeYearAsync(
            empId.Value, leaveType, command.StartDate.Year, ct);

        if (balance != null)
            balance.Deduct(leaveRequest.TotalDays);

        _leaveRequestRepository.Add(leaveRequest);
        await _unitOfWork.SaveChangesAsync(ct);
        return leaveRequest.Id;
    }
}
