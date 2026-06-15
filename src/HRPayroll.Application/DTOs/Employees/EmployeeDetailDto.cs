using HRPayroll.Application.DTOs.Contracts;

namespace HRPayroll.Application.DTOs.Employees;

public record EmployeeDetailDto : EmployeeDto
{
    public DateOnly DateOfBirth { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public string? PersonalEmail { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public ContractDto? ActiveContract { get; init; }
    public DateOnly? TerminationDate { get; init; }
}
