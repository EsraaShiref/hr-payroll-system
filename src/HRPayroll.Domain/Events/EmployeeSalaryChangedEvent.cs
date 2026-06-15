using MediatR;
using HRPayroll.Domain.ValueObjects;

namespace HRPayroll.Domain.Events;

public sealed record EmployeeSalaryChangedEvent(
    Guid EmployeeId,
    Guid ContractId,
    Money PreviousBaseSalary,
    Money NewBaseSalary,
    DateOnly EffectiveFrom,
    DateTime OccurredAt) : INotification;
