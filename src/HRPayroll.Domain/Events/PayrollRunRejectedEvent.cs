using MediatR;

namespace HRPayroll.Domain.Events;

public sealed record PayrollRunRejectedEvent(
    Guid PayrollRunId,
    string RejectedBy,
    string Reason,
    DateTime OccurredAt) : INotification;
