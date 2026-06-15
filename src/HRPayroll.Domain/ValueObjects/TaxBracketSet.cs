using HRPayroll.Domain.Exceptions;

namespace HRPayroll.Domain.ValueObjects;

public sealed record TaxBracketSet
{
    public IReadOnlyList<TaxBracket> Brackets { get; }
    public DateOnly EffectiveFrom { get; }
    public DateOnly? EffectiveTo { get; }

    private TaxBracketSet(IReadOnlyList<TaxBracket> brackets, DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        if (brackets == null || brackets.Count == 0)
            throw new ArgumentException("At least one tax bracket is required.", nameof(brackets));
        if (effectiveTo.HasValue && effectiveTo <= effectiveFrom)
            throw new ArgumentException("EffectiveTo must be after EffectiveFrom.", nameof(effectiveTo));

        Brackets = brackets;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public static TaxBracketSet Create(IReadOnlyList<TaxBracket> brackets, DateOnly effectiveFrom, DateOnly? effectiveTo)
        => new(brackets, effectiveFrom, effectiveTo);

    public decimal CalculateTax(decimal taxableIncome)
    {
        if (taxableIncome < 0)
            throw new ArgumentException("Taxable income cannot be negative.", nameof(taxableIncome));

        if (!IsEffectiveFor(taxableIncome))
            throw new InvalidOperationException("Taxable income must fall within a bracket range.");

        decimal totalTax = 0;
        decimal previousUpper = 0;

        foreach (var bracket in Brackets.OrderBy(b => b.FromAmount))
        {
            if (taxableIncome <= previousUpper)
                break;

            decimal bracketUpper = bracket.ToAmount ?? decimal.MaxValue;
            decimal taxableInBracket = Math.Min(taxableIncome, bracketUpper) - previousUpper;

            if (taxableInBracket > 0)
                totalTax += taxableInBracket * bracket.Rate / 100m;

            previousUpper = bracketUpper;
        }

        return totalTax;
    }

    private bool IsEffectiveFor(decimal taxableIncome)
    {
        return Brackets.Any(b => taxableIncome >= b.FromAmount && (!b.ToAmount.HasValue || taxableIncome <= b.ToAmount.Value));
    }

    public bool IsCurrentAsOf(DateOnly date) =>
        EffectiveFrom <= date && (!EffectiveTo.HasValue || EffectiveTo.Value > date);
}
