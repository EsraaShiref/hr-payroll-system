namespace HRPayroll.Domain.ValueObjects;

public sealed record TaxBracket
{
    public decimal FromAmount { get; }
    public decimal? ToAmount { get; }
    public decimal Rate { get; }

    private TaxBracket(decimal fromAmount, decimal? toAmount, decimal rate)
    {
        if (fromAmount < 0)
            throw new ArgumentException("FromAmount cannot be negative.", nameof(fromAmount));
        if (toAmount.HasValue && toAmount <= fromAmount)
            throw new ArgumentException("ToAmount must be greater than FromAmount.", nameof(toAmount));
        if (rate < 0 || rate > 100)
            throw new ArgumentException("Rate must be between 0 and 100.", nameof(rate));

        FromAmount = fromAmount;
        ToAmount = toAmount;
        Rate = rate;
    }

    public static TaxBracket Create(decimal fromAmount, decimal? toAmount, decimal rate)
        => new(fromAmount, toAmount, rate);
}
