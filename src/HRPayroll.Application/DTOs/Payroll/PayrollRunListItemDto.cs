using HRPayroll.Domain.Enums;

namespace HRPayroll.Application.DTOs.Payroll;

public record PayrollRunListItemDto
{
    public Guid Id { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public PayrollRunStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string ProcessedBy { get; init; } = string.Empty;
    public int TotalEmployees { get; init; }
    public decimal TotalNetPay { get; init; }
}
