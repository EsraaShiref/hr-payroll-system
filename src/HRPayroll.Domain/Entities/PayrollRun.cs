using HRPayroll.Domain.Enums;
using HRPayroll.Domain.Events;
using HRPayroll.Domain.Exceptions;
using MediatR;

namespace HRPayroll.Domain.Entities;

public class PayrollRun : BaseEntity
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public PayrollRunStatus Status { get; private set; }

    public byte[]? RowVersion { get; private set; }

    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string ProcessedBy { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    private readonly List<PayrollRunDetail> _details = new();
    public IReadOnlyCollection<PayrollRunDetail> Details => _details.AsReadOnly();

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private PayrollRun() { }

    public PayrollRun(int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentException("Year must be between 2000 and 2100.", nameof(year));
        if (month < 1 || month > 12)
            throw new ArgumentException("Month must be between 1 and 12.", nameof(month));

        Year = year;
        Month = month;
        Status = PayrollRunStatus.Draft;
    }

    public void StartProcessing(string processedBy)
    {
        if (Status != PayrollRunStatus.Draft)
            throw new InvalidPayrollRunStateException(
                $"Cannot start processing a run with status {Status}. Must be Draft.");

        Status = PayrollRunStatus.Processing;
        ProcessedBy = processedBy;
        StartedAt = DateTime.UtcNow;

        _domainEvents.Add(new PayrollRunStartedEvent(Id, Year, Month, processedBy, DateTime.UtcNow));
    }

    public void CompletePendingReview()
    {
        if (Status != PayrollRunStatus.Processing)
            throw new InvalidPayrollRunStateException(
                $"Cannot complete processing a run with status {Status}. Must be Processing.");

        Status = PayrollRunStatus.PendingReview;
        CompletedAt = DateTime.UtcNow;

        var calculated = _details.Count(d => d.Status == PayrollRunDetailStatus.Calculated);
        var skipped = _details.Count(d => d.Status == PayrollRunDetailStatus.Skipped);
        var failed = _details.Count(d => d.Status == PayrollRunDetailStatus.Failed);

        _domainEvents.Add(new PayrollRunCompletedEvent(
            Id, _details.Count, calculated, skipped, failed, DateTime.UtcNow));
    }

    public void Approve()
    {
        if (Status != PayrollRunStatus.PendingReview)
            throw new InvalidPayrollRunStateException(
                $"Cannot approve a run with status {Status}. Must be PendingReview.");

        Status = PayrollRunStatus.Approved;
    }

    public void FinalizeRun()
    {
        if (Status != PayrollRunStatus.Approved)
            throw new InvalidPayrollRunStateException(
                $"Cannot finalize a run with status {Status}. Must be Approved.");

        var failedCount = _details.Count(d => d.Status == PayrollRunDetailStatus.Failed);
        if (failedCount > 0)
            throw new PayrollRunAlreadyFinalizedException(
                $"Cannot finalize a run with {failedCount} failed detail(s). Resolve or re-run before finalizing.");

        Status = PayrollRunStatus.Finalized;

        _domainEvents.Add(new PayrollRunFinalizedEvent(Id, Year, Month, DateTime.UtcNow));
    }

    public void Reject(string rejectedBy, string reason)
    {
        if (Status != PayrollRunStatus.PendingReview)
            throw new InvalidPayrollRunStateException(
                $"Cannot reject a run with status {Status}. Must be PendingReview.");

        Status = PayrollRunStatus.Draft;

        _domainEvents.Add(new PayrollRunRejectedEvent(Id, rejectedBy, reason, DateTime.UtcNow));
    }

    public bool CanAddDetail() => Status == PayrollRunStatus.Processing;

    public void AddDetail(PayrollRunDetail detail)
    {
        if (!CanAddDetail())
            throw new InvalidPayrollRunStateException(
                "Cannot add details to a run that is not in Processing status.");

        if (detail == null)
            throw new ArgumentNullException(nameof(detail));

        if (_details.Any(d => d.EmployeeId == detail.EmployeeId))
            throw new InvalidOperationException(
                $"Detail for employee {detail.EmployeeId} already exists in this run.");

        detail.SetPayrollRunId(Id);
        _details.Add(detail);
    }

    public IReadOnlyCollection<PayrollRunDetail> GetSkippedDetails()
        => _details.Where(d => d.Status == PayrollRunDetailStatus.Skipped).ToList().AsReadOnly();

    public IReadOnlyCollection<PayrollRunDetail> GetFailedDetails()
        => _details.Where(d => d.Status == PayrollRunDetailStatus.Failed).ToList().AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
}
