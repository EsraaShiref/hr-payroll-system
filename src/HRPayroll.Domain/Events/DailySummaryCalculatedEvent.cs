using MediatR;

namespace HRPayroll.Domain.Events;

public record DailySummaryCalculatedEvent(
    Guid SummaryId,
    Guid EmployeeId,
    DateOnly Date,
    string Status,
    bool IsUnexcusedAbsence,
    bool IsOnLeave,
    bool IsHoliday,
    DateTime OccurredAt) : INotification;
