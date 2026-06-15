namespace HRPayroll.Application.DTOs.Allowances;

public record AllowanceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal? DefaultAmount { get; init; }
    public decimal? DefaultPercentage { get; init; }
    public string Taxability { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
