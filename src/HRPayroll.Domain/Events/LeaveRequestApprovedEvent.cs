using MediatR;

namespace HRPayroll.Domain.Events;

public record LeaveRequestApprovedEvent(
    Guid LeaveRequestId,
    Guid EmployeeId,
    Guid ApprovedBy,
    DateTime OccurredAt) : INotification;
