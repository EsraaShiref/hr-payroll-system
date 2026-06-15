using MediatR;

namespace HRPayroll.Domain.Events;

public sealed record EmployeeTerminatedEvent(
    Guid EmployeeId,
    DateOnly TerminationDate,
    string Reason,
    DateTime OccurredAt) : INotification;
