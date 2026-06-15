using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

public sealed record PayrollCalculationRequest
{
    public Guid PayrollRunId { get; init; }
    public Guid EmployeeId { get; init; }
    public Guid ContractVersionId { get; init; }
    public decimal BaseSalary { get; init; }
    public decimal OvertimeRateMultiplier { get; init; }
    public int WorkingDaysPerMonth { get; init; }
    public decimal StandardDailyHours { get; init; }
    public IReadOnlyList<AttendanceDailySummary> AttendanceSummaries { get; init; } = Array.Empty<AttendanceDailySummary>();
    public TaxBracketSet? TaxBracketSet { get; init; }
    public SocialInsuranceConfig? SocialInsuranceConfig { get; init; }
    public PayrollPolicy Policy { get; init; } = null!;
    public LeaveBalance? LeaveBalance { get; init; }
    public int LeaveBalanceSnapshotDays { get; init; }
    public decimal TotalAllowances { get; init; }
    public decimal NonTaxableAllowancesTotal { get; init; }
    public string CalculatedBy { get; init; } = string.Empty;
}
