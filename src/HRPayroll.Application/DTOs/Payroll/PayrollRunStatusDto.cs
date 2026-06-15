using HRPayroll.Domain.Enums;

namespace HRPayroll.Application.DTOs.Payroll;

public record PayrollRunStatusDto
{
    public Guid Id { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public PayrollRunStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int TotalEmployees { get; init; }
    public int CalculatedCount { get; init; }
    public int SkippedCount { get; init; }
    public int FailedCount { get; init; }
}
