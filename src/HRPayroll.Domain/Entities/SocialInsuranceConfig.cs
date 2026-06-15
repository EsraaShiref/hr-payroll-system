namespace HRPayroll.Domain.Entities;

public class SocialInsuranceConfig : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal EmployeeContributionPercent { get; private set; }
    public decimal EmployerContributionPercent { get; private set; }
    public decimal MaxContributionSalary { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    private SocialInsuranceConfig() { }

    public SocialInsuranceConfig(
        string name,
        decimal employeePct,
        decimal employerPct,
        decimal maxSalary,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        SetName(name);
        SetContributions(employeePct, employerPct, maxSalary);
        SetEffectiveRange(effectiveFrom, effectiveTo);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Social insurance config name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters.", nameof(name));
        Name = name;
    }

    public void SetContributions(decimal employeePct, decimal employerPct, decimal maxSalary)
    {
        if (employeePct < 0 || employeePct > 100)
            throw new ArgumentException("Employee contribution must be between 0 and 100.", nameof(employeePct));
        if (employerPct < 0 || employerPct > 100)
            throw new ArgumentException("Employer contribution must be between 0 and 100.", nameof(employerPct));
        if (maxSalary < 0)
            throw new ArgumentException("Max contribution salary cannot be negative.", nameof(maxSalary));

        EmployeeContributionPercent = employeePct;
        EmployerContributionPercent = employerPct;
        MaxContributionSalary = maxSalary;
    }

    public void SetEffectiveRange(DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        if (effectiveTo.HasValue && effectiveTo <= effectiveFrom)
            throw new ArgumentException("EffectiveTo must be after EffectiveFrom.");
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public decimal CalculateEmployeeContribution(decimal grossSalary)
    {
        var cappedSalary = Math.Min(grossSalary, MaxContributionSalary);
        return cappedSalary * EmployeeContributionPercent / 100m;
    }

    public decimal CalculateEmployerContribution(decimal grossSalary)
    {
        var cappedSalary = Math.Min(grossSalary, MaxContributionSalary);
        return cappedSalary * EmployerContributionPercent / 100m;
    }

    public bool IsCurrentAsOf(DateOnly date) =>
        EffectiveFrom <= date && (!EffectiveTo.HasValue || EffectiveTo.Value > date);
}
