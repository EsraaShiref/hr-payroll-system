namespace HRPayroll.Domain.ValueObjects;

public sealed record SocialInsuranceConfig
{
    public decimal EmployeeContributionPercent { get; }
    public decimal EmployerContributionPercent { get; }
    public decimal MaxContributionSalary { get; }
    public DateOnly EffectiveFrom { get; }
    public DateOnly? EffectiveTo { get; }

    private SocialInsuranceConfig(decimal employeePct, decimal employerPct, decimal maxSalary, DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        if (employeePct < 0 || employeePct > 100)
            throw new ArgumentException("Employee contribution must be between 0 and 100.", nameof(employeePct));
        if (employerPct < 0 || employerPct > 100)
            throw new ArgumentException("Employer contribution must be between 0 and 100.", nameof(employerPct));
        if (maxSalary < 0)
            throw new ArgumentException("Max contribution salary cannot be negative.", nameof(maxSalary));
        if (effectiveTo.HasValue && effectiveTo <= effectiveFrom)
            throw new ArgumentException("EffectiveTo must be after EffectiveFrom.", nameof(effectiveTo));

        EmployeeContributionPercent = employeePct;
        EmployerContributionPercent = employerPct;
        MaxContributionSalary = maxSalary;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public static SocialInsuranceConfig Create(decimal employeePct, decimal employerPct, decimal maxSalary, DateOnly effectiveFrom, DateOnly? effectiveTo)
        => new(employeePct, employerPct, maxSalary, effectiveFrom, effectiveTo);

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
