using MediatR;

namespace HRPayroll.Domain.Events;

public record LeaveRequestSubmittedEvent(
    Guid LeaveRequestId,
    Guid EmployeeId,
    string LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    DateTime OccurredAt) : INotification;
