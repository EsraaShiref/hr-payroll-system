using MediatR;

namespace HRPayroll.Domain.Events;

public sealed record PayrollRunStartedEvent(
    Guid PayrollRunId,
    int Year,
    int Month,
    string StartedBy,
    DateTime OccurredAt) : INotification;
