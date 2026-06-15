using MediatR;

namespace HRPayroll.Domain.Events;

public record LeaveRequestRejectedEvent(
    Guid LeaveRequestId,
    Guid EmployeeId,
    Guid RejectedBy,
    string Reason,
    DateTime OccurredAt) : INotification;
