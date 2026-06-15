namespace HRPayroll.Application.DTOs.Employees;

public record EmployeeDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName}{(MiddleName != null ? $" {MiddleName}" : "")} {LastName}";
    public string? DepartmentName { get; init; }
    public string? PositionTitle { get; init; }
    public string EmploymentStatus { get; init; } = string.Empty;
    public DateOnly HireDate { get; init; }
}
