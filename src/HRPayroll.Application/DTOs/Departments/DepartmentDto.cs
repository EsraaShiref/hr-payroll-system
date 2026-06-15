namespace HRPayroll.Application.DTOs.Departments;

public record DepartmentDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentDepartmentId { get; init; }
    public string? ParentDepartmentName { get; init; }
    public int EmployeeCount { get; init; }
}
