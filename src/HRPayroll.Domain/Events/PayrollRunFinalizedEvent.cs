using MediatR;

namespace HRPayroll.Domain.Events;

public sealed record PayrollRunFinalizedEvent(
    Guid PayrollRunId,
    int Year,
    int Month,
    DateTime FinalizedAt) : INotification;
