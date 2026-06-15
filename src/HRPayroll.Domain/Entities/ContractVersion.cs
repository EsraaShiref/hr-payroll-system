using HRPayroll.Domain.Exceptions;
using HRPayroll.Domain.ValueObjects;

namespace HRPayroll.Domain.Entities;

public class ContractVersion : BaseEntity
{
    public Guid ContractId { get; private set; }
    public Contract Contract { get; private set; } = null!;
    public int VersionNumber { get; private set; }
    public Money BaseSalary { get; private set; } = null!;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public Guid? TaxBracketSetId { get; private set; }
    public Guid? SocialInsuranceConfigId { get; private set; }
    public decimal? OvertimeRateMultiplier { get; private set; }
    private readonly List<AllowanceAssignment> _allowanceAssignments = new();
    public IReadOnlyCollection<AllowanceAssignment> AllowanceAssignments => _allowanceAssignments.AsReadOnly();

    private ContractVersion() { }

    public ContractVersion(
        int versionNumber,
        Money baseSalary,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? taxBracketSetId,
        Guid? socialInsuranceConfigId,
        decimal? overtimeRateMultiplier)
    {
        if (versionNumber < 1)
            throw new ArgumentException("Version number must be 1 or greater.", nameof(versionNumber));
        if (baseSalary.Amount <= 0)
            throw new InvalidSalaryException(baseSalary.Amount);
        if (effectiveTo.HasValue && effectiveTo <= effectiveFrom)
            throw new InvalidContractDateRangeException("EffectiveTo must be after EffectiveFrom.");
        if (overtimeRateMultiplier.HasValue && (overtimeRateMultiplier.Value < 1.0m || overtimeRateMultiplier.Value > 10.0m))
            throw new ArgumentException("Overtime rate multiplier must be between 1.0 and 10.0.", nameof(overtimeRateMultiplier));

        VersionNumber = versionNumber;
        BaseSalary = baseSalary;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        TaxBracketSetId = taxBracketSetId;
        SocialInsuranceConfigId = socialInsuranceConfigId;
        OvertimeRateMultiplier = overtimeRateMultiplier;
    }

    public void AddAllowanceAssignment(AllowanceAssignment assignment)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));
        _allowanceAssignments.Add(assignment);
    }

    public void Close(DateOnly closedAt)
    {
        if (EffectiveTo.HasValue)
            throw new InvalidContractDateRangeException("Version is already closed.");
        if (closedAt <= EffectiveFrom)
            throw new InvalidContractDateRangeException("Close date must be after EffectiveFrom.");

        EffectiveTo = closedAt;
    }

    public bool IsEffectiveOn(DateOnly date) =>
        EffectiveFrom <= date && (!EffectiveTo.HasValue || EffectiveTo > date);
}
