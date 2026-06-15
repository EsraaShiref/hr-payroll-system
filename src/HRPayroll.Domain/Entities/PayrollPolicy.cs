namespace HRPayroll.Domain.Entities;

public class PayrollPolicy : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    public int WorkingDaysPerMonth { get; private set; }
    public decimal StandardDailyHours { get; private set; }
    public int LateOccurrencesThreshold { get; private set; }
    public decimal DefaultOvertimeRateMultiplier { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";

    private PayrollPolicy() { }

    public PayrollPolicy(
        string name,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        int workingDaysPerMonth,
        decimal standardDailyHours,
        int lateOccurrencesThreshold,
        decimal defaultOvertimeRateMultiplier,
        string currencyCode)
    {
        SetName(name);
        SetEffectiveRange(effectiveFrom, effectiveTo);
        WorkingDaysPerMonth = workingDaysPerMonth;
        StandardDailyHours = standardDailyHours;
        LateOccurrencesThreshold = lateOccurrencesThreshold;
        DefaultOvertimeRateMultiplier = defaultOvertimeRateMultiplier;
        CurrencyCode = currencyCode;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Payroll policy name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters.", nameof(name));
        Name = name;
    }

    public void SetEffectiveRange(DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        if (effectiveTo.HasValue && effectiveTo <= effectiveFrom)
            throw new ArgumentException("EffectiveTo must be after EffectiveFrom.");
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public bool IsCurrentAsOf(DateOnly date) =>
        EffectiveFrom <= date && (!EffectiveTo.HasValue || EffectiveTo.Value > date);
}
