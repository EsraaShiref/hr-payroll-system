using HRPayroll.Domain.Exceptions;
using HRPayroll.Domain.ValueObjects;

namespace HRPayroll.Domain.Entities;

public class TaxBracketSet : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    private readonly List<TaxBracket> _brackets = new();
    public IReadOnlyList<TaxBracket> Brackets => _brackets.AsReadOnly();
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    private TaxBracketSet() { }

    public TaxBracketSet(string name, IEnumerable<TaxBracket> brackets, DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        SetName(name);
        SetBrackets(brackets);
        SetEffectiveRange(effectiveFrom, effectiveTo);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tax bracket set name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters.", nameof(name));
        Name = name;
    }

    public void SetBrackets(IEnumerable<TaxBracket> brackets)
    {
        var list = brackets?.ToList()
            ?? throw new ArgumentNullException(nameof(brackets));
        if (list.Count == 0)
            throw new ArgumentException("At least one tax bracket is required.", nameof(brackets));
        _brackets.Clear();
        _brackets.AddRange(list.OrderBy(b => b.FromAmount));
    }

    public void SetEffectiveRange(DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        if (effectiveTo.HasValue && effectiveTo <= effectiveFrom)
            throw new InvalidContractDateRangeException("EffectiveTo must be after EffectiveFrom.");
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public decimal CalculateTax(decimal taxableIncome)
    {
        if (taxableIncome < 0)
            throw new ArgumentException("Taxable income cannot be negative.", nameof(taxableIncome));

        decimal totalTax = 0;
        decimal previousUpper = 0;

        foreach (var bracket in _brackets.OrderBy(b => b.FromAmount))
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

    public bool IsCurrentAsOf(DateOnly date) =>
        EffectiveFrom <= date && (!EffectiveTo.HasValue || EffectiveTo.Value > date);
}
