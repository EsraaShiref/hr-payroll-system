namespace HRPayroll.Application.DTOs.Contracts;

public record ContractDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string ContractType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateOnly SignedDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public ContractVersionDto CurrentVersion { get; init; } = null!;
    public DateOnly? TerminationDate { get; init; }
}
