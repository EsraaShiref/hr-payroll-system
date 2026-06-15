using HRPayroll.Domain.Enums;
using HRPayroll.Domain.Events;
using HRPayroll.Domain.Exceptions;
using HRPayroll.Domain.ValueObjects;
using MediatR;

namespace HRPayroll.Domain.Entities;

public class Contract : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public ContractType ContractType { get; private set; }
    public ContractStatus Status { get; private set; }
    public DateOnly SignedDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public DateOnly? TerminationDate { get; private set; }
    public string? TerminationReason { get; private set; }
    public ICollection<ContractVersion> Versions { get; private set; } = new List<ContractVersion>();

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private Contract() { }

    public Contract(
        Guid employeeId,
        ContractType contractType,
        DateOnly signedDate,
        DateOnly? expiryDate,
        ContractVersion initialVersion)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId is required.", nameof(employeeId));
        if (signedDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("SignedDate cannot be in the future.", nameof(signedDate));
        if (contractType == ContractType.FixedTerm && !expiryDate.HasValue)
            throw new ArgumentException("ExpiryDate is required for FixedTerm contracts.", nameof(expiryDate));
        if (expiryDate.HasValue && expiryDate <= signedDate)
            throw new InvalidContractDateRangeException("ExpiryDate must be after SignedDate.");

        EmployeeId = employeeId;
        ContractType = contractType;
        Status = ContractStatus.Draft;
        SignedDate = signedDate;
        ExpiryDate = expiryDate;
        Versions.Add(initialVersion ?? throw new ArgumentNullException(nameof(initialVersion)));
    }

    public ContractVersion CurrentVersion =>
        Versions
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Contract has no versions.");

    public ContractVersion? GetVersionEffectiveOn(DateOnly date) =>
        Versions.FirstOrDefault(v => v.IsEffectiveOn(date));

    public void Activate()
    {
        if (Status == ContractStatus.Terminated)
            throw new ContractAlreadyTerminatedException(Id);

        Status = ContractStatus.Active;
        _domainEvents.Add(new ContractActivatedEvent(Id, EmployeeId, CurrentVersion.EffectiveFrom, DateTime.UtcNow));
    }

    public void Terminate(DateOnly terminationDate, string reason)
    {
        if (Status == ContractStatus.Terminated)
            throw new ContractAlreadyTerminatedException(Id);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Termination reason is required.", nameof(reason));

        Status = ContractStatus.Terminated;
        TerminationDate = terminationDate;
        TerminationReason = reason;

        var currentVersion = CurrentVersion;
        if (currentVersion.IsEffectiveOn(terminationDate))
            currentVersion.Close(terminationDate);

        _domainEvents.Add(new ContractTerminatedEvent(Id, EmployeeId, terminationDate, reason, DateTime.UtcNow));
    }

    public ContractVersion AddVersion(
        Money newBaseSalary,
        DateOnly effectiveFrom,
        Guid? taxBracketSetId,
        Guid? socialInsuranceConfigId,
        IEnumerable<AllowanceAssignment>? allowanceAssignments)
    {
        if (Status == ContractStatus.Terminated)
            throw new ContractAlreadyTerminatedException(Id);

        var nextVersionNumber = (Versions.MaxBy(v => v.VersionNumber)?.VersionNumber ?? 0) + 1;

        var previousVersion = CurrentVersion;
        if (previousVersion != null && previousVersion.IsEffectiveOn(effectiveFrom))
        {
            previousVersion.Close(effectiveFrom);
        }

        var newVersion = new ContractVersion(
            nextVersionNumber,
            newBaseSalary,
            effectiveFrom,
            null,
            taxBracketSetId,
            socialInsuranceConfigId,
            allowanceAssignments);

        Versions.Add(newVersion);

        _domainEvents.Add(new EmployeeSalaryChangedEvent(
            EmployeeId, Id, previousVersion?.BaseSalary ?? Money.Zero(newBaseSalary.Currency),
            newBaseSalary, effectiveFrom, DateTime.UtcNow));

        return newVersion;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
