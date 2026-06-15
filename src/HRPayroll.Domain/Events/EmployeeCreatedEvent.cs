using MediatR;

namespace HRPayroll.Domain.Events;

public sealed record EmployeeCreatedEvent(
    Guid EmployeeId,
    string EmployeeCode,
    Guid DepartmentId,
    Guid PositionId,
    DateTime OccurredAt) : INotification;
