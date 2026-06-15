namespace HRPayroll.Domain.ValueObjects;

public sealed record Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(string street, string city, string? state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required.", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required.", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required.", nameof(country));

        Street = street;
        City = city;
        State = state ?? string.Empty;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string street, string city, string? state, string postalCode, string country)
        => new(street, city, state, postalCode, country);

    public override string ToString() =>
        $"{Street}, {City}{(string.IsNullOrEmpty(State) ? "" : $", {State}")} {PostalCode}, {Country}";
}
