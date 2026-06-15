using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Events;
using MediatR;

namespace HRPayroll.Application.EventHandlers;

public class LeaveRequestRejectedEventHandler : INotificationHandler<LeaveRequestRejectedEvent>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveRequestRejectedEventHandler(
        ILeaveRequestRepository leaveRequestRepository,
        IAttendanceSummaryRepository summaryRepository,
        IUnitOfWork unitOfWork)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _summaryRepository = summaryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LeaveRequestRejectedEvent notification, CancellationToken ct)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(notification.LeaveRequestId, ct);
        if (leaveRequest == null)
            return;

        for (var date = leaveRequest.StartDate; date <= leaveRequest.EndDate; date = date.AddDays(1))
        {
            var summary = await _summaryRepository.GetByEmployeeAndDateAsync(
                leaveRequest.EmployeeId, date, ct);

            if (summary != null && summary.LeaveRequestId == leaveRequest.Id)
            {
                summary.ClearLeave();
                summary.Calculate();
                _summaryRepository.Update(summary);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
