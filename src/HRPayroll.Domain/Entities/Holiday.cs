namespace HRPayroll.Domain.Entities;

public class Holiday : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public bool IsRecurringYearly { get; private set; }

    private Holiday() { }

    public Holiday(string name, DateOnly date, bool isRecurringYearly = false)
    {
        SetName(name);
        Date = date;
        IsRecurringYearly = isRecurringYearly;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Holiday name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Holiday name cannot exceed 200 characters.", nameof(name));
        Name = name.Trim();
    }
}
