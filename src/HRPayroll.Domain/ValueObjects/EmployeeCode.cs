namespace HRPayroll.Domain.ValueObjects;

public sealed record EmployeeCode
{
    public string Value { get; }

    private EmployeeCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Employee code cannot be empty.", nameof(value));
        if (value.Length > 20)
            throw new ArgumentException("Employee code cannot exceed 20 characters.", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    public static EmployeeCode Create(string value) => new(value);

    public override string ToString() => Value;
}
