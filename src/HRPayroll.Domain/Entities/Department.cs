namespace HRPayroll.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentDepartmentId { get; private set; }
    public Department? ParentDepartment { get; private set; }
    public Guid? DefaultShiftId { get; private set; }
    public Shift? DefaultShift { get; private set; }
    public ICollection<Department> SubDepartments { get; private set; } = new List<Department>();
    public ICollection<Position> Positions { get; private set; } = new List<Position>();
    public ICollection<Employee> Employees { get; private set; } = new List<Employee>();

    private Department() { }

    public Department(string name, string code, string? description, Guid? parentDepartmentId)
    {
        SetName(name);
        SetCode(code);
        Description = description;
        ParentDepartmentId = parentDepartmentId;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Department name cannot exceed 200 characters.", nameof(name));
        Name = name;
    }

    public void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Department code is required.", nameof(code));
        if (code.Length > 20)
            throw new ArgumentException("Department code cannot exceed 20 characters.", nameof(code));
        Code = code.Trim().ToUpperInvariant();
    }

    public void SetDescription(string? description) => Description = description;

    public void ChangeParent(Guid? parentDepartmentId) => ParentDepartmentId = parentDepartmentId;

    public void AssignDefaultShift(Guid shiftId) => DefaultShiftId = shiftId;

    public void RemoveDefaultShift() => DefaultShiftId = null;

    public void AddPosition(Position position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));
        Positions.Add(position);
    }
}
