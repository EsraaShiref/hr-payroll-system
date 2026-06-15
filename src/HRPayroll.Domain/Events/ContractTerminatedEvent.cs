using MediatR;

namespace HRPayroll.Domain.Events;

public sealed record ContractTerminatedEvent(
    Guid ContractId,
    Guid EmployeeId,
    DateOnly TerminationDate,
    string Reason,
    DateTime OccurredAt) : INotification;
