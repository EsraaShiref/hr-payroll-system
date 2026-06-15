using MediatR;

namespace HRPayroll.Domain.Events;

public record PunchRecordedEvent(
    Guid PunchId,
    Guid EmployeeId,
    DateTime TimestampUtc,
    string PunchType,
    string Source,
    DateTime OccurredAt) : INotification;
