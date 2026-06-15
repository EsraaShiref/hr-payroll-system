using HRPayroll.Domain.Enums;

namespace HRPayroll.Application.DTOs.Payroll;

public record PayrollRunSummaryDto
{
    public Guid Id { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public PayrollRunStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string ProcessedBy { get; init; } = string.Empty;
    public decimal TotalGrossPay { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal TotalNetPay { get; init; }
    public int TotalEmployees { get; init; }
    public int CalculatedCount { get; init; }
    public int SkippedCount { get; init; }
    public int FailedCount { get; init; }
    public List<SkippedEmployeeInfo> SkippedEmployees { get; init; } = new();
    public List<FailedEmployeeInfo> FailedEmployees { get; init; } = new();
}

public record SkippedEmployeeInfo(Guid EmployeeId, string EmployeeName, string SkipReason);

public record FailedEmployeeInfo(Guid EmployeeId, string EmployeeName, string FailureMessage);
