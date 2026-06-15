namespace HRPayroll.Domain.Entities;

public class Position : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Department Department { get; private set; } = null!;
    public ICollection<Employee> Employees { get; private set; } = new List<Employee>();

    private Position() { }

    public Position(string title, string code, string? description, Guid departmentId)
    {
        SetTitle(title);
        SetCode(code);
        Description = description;
        DepartmentId = departmentId;
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Position title is required.", nameof(title));
        if (title.Length > 200)
            throw new ArgumentException("Position title cannot exceed 200 characters.", nameof(title));
        Title = title;
    }

    public void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Position code is required.", nameof(code));
        if (code.Length > 20)
            throw new ArgumentException("Position code cannot exceed 20 characters.", nameof(code));
        Code = code.Trim().ToUpperInvariant();
    }

    public void SetDescription(string? description) => Description = description;
}
