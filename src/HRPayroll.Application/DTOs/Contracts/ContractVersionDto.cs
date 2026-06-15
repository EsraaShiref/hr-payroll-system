namespace HRPayroll.Application.DTOs.Contracts;

public record ContractVersionDto
{
    public Guid Id { get; init; }
    public int VersionNumber { get; init; }
    public decimal BaseSalaryAmount { get; init; }
    public string BaseSalaryCurrency { get; init; } = "USD";
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public List<AllowanceAssignmentDto> AllowanceAssignments { get; init; } = new();
}

public record AllowanceAssignmentDto
{
    public Guid AllowanceId { get; init; }
    public string AllowanceName { get; init; } = string.Empty;
    public string AllowanceType { get; init; } = string.Empty;
    public decimal? OverrideAmount { get; init; }
    public decimal? OverridePercentage { get; init; }
    public decimal ComputedValue { get; init; }
}
