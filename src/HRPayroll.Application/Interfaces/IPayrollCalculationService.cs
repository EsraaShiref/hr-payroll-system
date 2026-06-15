using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

/// <summary>
/// Pure computation service for per-employee payroll calculation.
/// All data is passed in — no DB queries inside.
/// The parameter list is explicit (~10 params) to keep the calculation
/// deterministic and unit-testable without a database.
/// If parameter-list complexity is a concern, can be refactored to use
/// a sealed PayrollCalculationRequest record.
/// </summary>
public interface IPayrollCalculationService
{
    Task<PayrollRunDetail> CalculateEmployeePayrollAsync(
        Guid payrollRunId,
        Guid employeeId,
        Guid contractVersionId,
        decimal baseSalary,
        decimal overtimeRateMultiplier,
        int workingDaysPerMonth,
        decimal standardDailyHours,
        IReadOnlyList<AttendanceDailySummary> attendanceSummaries,
        TaxBracketSet? taxBracketSet,
        SocialInsuranceConfig? socialInsuranceConfig,
        PayrollPolicy policy,
        LeaveBalance? leaveBalance,
        int leaveBalanceSnapshotDays,
        string calculatedBy,
        CancellationToken ct = default);
}
