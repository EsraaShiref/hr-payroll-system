using HRPayroll.Domain.Enums;
using HRPayroll.Domain.Exceptions;
using HRPayroll.Domain.ValueObjects;

namespace HRPayroll.Domain.Entities;

public class AllowanceAssignment : BaseEntity
{
    public Guid ContractVersionId { get; private set; }
    public ContractVersion ContractVersion { get; private set; } = null!;
    public Guid AllowanceId { get; private set; }
    public Allowance Allowance { get; private set; } = null!;
    public decimal? OverrideAmount { get; private set; }
    public decimal? OverridePercentage { get; private set; }

    private AllowanceAssignment() { }

    public AllowanceAssignment(Guid contractVersionId, Guid allowanceId, decimal? overrideAmount, decimal? overridePercentage)
    {
        ContractVersionId = contractVersionId;
        AllowanceId = allowanceId;
        SetOverrides(overrideAmount, overridePercentage);
    }

    public void SetOverrides(decimal? overrideAmount, decimal? overridePercentage)
    {
        if (overrideAmount.HasValue && overrideAmount <= 0)
            throw new ArgumentException("Override amount must be positive.", nameof(overrideAmount));
        if (overridePercentage.HasValue && (overridePercentage < 0 || overridePercentage > 100))
            throw new InvalidAllowancePercentageException(overridePercentage.Value);

        OverrideAmount = overrideAmount;
        OverridePercentage = overridePercentage;
    }

    public Money ComputeValue(Money baseSalary)
    {
        if (Allowance == null)
            throw new InvalidOperationException("Allowance must be loaded to compute value.");

        return Allowance.Type switch
        {
            AllowanceType.Fixed => Money.Create(
                OverrideAmount ?? Allowance.DefaultAmount
                    ?? throw new InvalidOperationException("No amount defined for fixed allowance."),
                baseSalary.Currency),

            AllowanceType.Percentage => baseSalary.MultiplyBy(
                (OverridePercentage ?? Allowance.DefaultPercentage
                    ?? throw new InvalidOperationException("No percentage defined for percentage allowance.")) / 100m),

            _ => throw new InvalidOperationException($"Unknown allowance type: {Allowance.Type}")
        };
    }
}
