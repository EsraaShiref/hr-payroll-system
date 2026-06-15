using MediatR;

namespace HRPayroll.Domain.Events;

public record AttendanceRecordedEvent(
    Guid AttendanceRecordId,
    Guid EmployeeId,
    DateOnly Date,
    string Status,
    DateTime OccurredAt) : INotification;
