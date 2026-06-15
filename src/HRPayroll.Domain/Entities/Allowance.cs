using HRPayroll.Domain.Enums;
using HRPayroll.Domain.Exceptions;

namespace HRPayroll.Domain.Entities;

public class Allowance : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AllowanceType Type { get; private set; }
    public decimal? DefaultAmount { get; private set; }
    public decimal? DefaultPercentage { get; private set; }
    public AllowanceTaxability Taxability { get; private set; }
    public bool IsActive { get; private set; }

    private Allowance() { }

    public Allowance(
        string name,
        string code,
        string? description,
        AllowanceType type,
        decimal? defaultAmount,
        decimal? defaultPercentage,
        AllowanceTaxability taxability)
    {
        SetName(name);
        SetCode(code);
        Description = description;
        SetType(type, defaultAmount, defaultPercentage);
        SetDefaultAmount(defaultAmount);
        SetDefaultPercentage(defaultPercentage);
        Taxability = taxability;
        IsActive = true;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Allowance name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Allowance name cannot exceed 200 characters.", nameof(name));
        Name = name;
    }

    public void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Allowance code is required.", nameof(code));
        if (code.Length > 20)
            throw new ArgumentException("Allowance code cannot exceed 20 characters.", nameof(code));
        Code = code.Trim().ToUpperInvariant();
    }

    public void SetType(AllowanceType type, decimal? defaultAmount, decimal? defaultPercentage)
    {
        Type = type;
        if (type == AllowanceType.Fixed && (!defaultAmount.HasValue || defaultAmount <= 0))
            throw new ArgumentException("Fixed allowance requires a positive default amount.", nameof(defaultAmount));
        if (type == AllowanceType.Percentage && (!defaultPercentage.HasValue || defaultPercentage <= 0 || defaultPercentage > 100))
            throw new InvalidAllowancePercentageException(defaultPercentage ?? 0);
    }

    public void SetDefaultAmount(decimal? amount) => DefaultAmount = amount;
    public void SetDefaultPercentage(decimal? percentage)
    {
        if (percentage.HasValue && (percentage < 0 || percentage > 100))
            throw new InvalidAllowancePercentageException(percentage.Value);
        DefaultPercentage = percentage;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
