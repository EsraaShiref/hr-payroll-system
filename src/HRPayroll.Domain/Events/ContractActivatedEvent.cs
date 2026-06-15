using MediatR;

namespace HRPayroll.Domain.Events;

public sealed record ContractActivatedEvent(
    Guid ContractId,
    Guid EmployeeId,
    DateOnly EffectiveFrom,
    DateTime OccurredAt) : INotification;
