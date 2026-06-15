using HRPayroll.Domain.Common;
using HRPayroll.Domain.Enums;
using HRPayroll.Domain.Events;
using HRPayroll.Domain.Exceptions;
using MediatR;

namespace HRPayroll.Domain.Entities;

public class LeaveRequest : BaseEntity, IHasDomainEvents
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public LeaveType LeaveType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal TotalDays { get; private set; }
    public LeaveRequestStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public Employee? ApprovedBy { get; private set; }
    public DateTime? ApprovalDate { get; private set; }
    public string? RejectionReason { get; private set; }

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private LeaveRequest() { }

    public LeaveRequest(Guid employeeId, LeaveType leaveType, DateOnly startDate, DateOnly endDate, string? reason)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be on or before end date.");

        EmployeeId = employeeId;
        LeaveType = leaveType;
        StartDate = startDate;
        EndDate = endDate;
        TotalDays = endDate.DayNumber - startDate.DayNumber + 1;
        Reason = reason;
        Status = LeaveRequestStatus.Pending;

        _domainEvents.Add(new LeaveRequestSubmittedEvent(
            Id, employeeId, leaveType.ToString(), startDate, endDate, DateTime.UtcNow));
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new InvalidLeaveRequestOperationException(
                $"Cannot approve a {Status} leave request.");

        Status = LeaveRequestStatus.Approved;
        ApprovedById = approvedBy;
        ApprovalDate = DateTime.UtcNow;

        _domainEvents.Add(new LeaveRequestApprovedEvent(
            Id, EmployeeId, approvedBy, DateTime.UtcNow));
    }

    public void Reject(Guid rejectedBy, string reason)
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new InvalidLeaveRequestOperationException(
                $"Cannot reject a {Status} leave request.");

        Status = LeaveRequestStatus.Rejected;
        RejectionReason = reason;
        ApprovalDate = DateTime.UtcNow;

        _domainEvents.Add(new LeaveRequestRejectedEvent(
            Id, EmployeeId, rejectedBy, reason, DateTime.UtcNow));
    }

    public void Cancel()
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new InvalidLeaveRequestOperationException(
                $"Cannot cancel a {Status} leave request.");

        Status = LeaveRequestStatus.Cancelled;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
