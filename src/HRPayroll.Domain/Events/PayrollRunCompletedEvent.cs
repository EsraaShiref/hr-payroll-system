using MediatR;

namespace HRPayroll.Domain.Events;

public sealed record PayrollRunCompletedEvent(
    Guid PayrollRunId,
    int TotalEmployees,
    int CalculatedCount,
    int SkippedCount,
    int FailedCount,
    DateTime OccurredAt) : INotification;
