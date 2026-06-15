using HRPayroll.Domain.Enums;

namespace HRPayroll.Domain.Entities;

public class PayrollRunDetail : BaseEntity
{
    public Guid PayrollRunId { get; private set; }
    public PayrollRun PayrollRun { get; private set; } = null!;

    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;

    // Status
    public PayrollRunDetailStatus Status { get; private set; }
    public SkipReason? SkipReason { get; private set; }
    public string? FailureMessage { get; private set; }

    // Snapshot FKs
    public Guid ContractVersionId { get; private set; }
    public Guid? TaxBracketSetId { get; private set; }
    public Guid? SocialInsuranceConfigId { get; private set; }
    public Guid? PayrollPolicyId { get; private set; }

    // Snapshot values
    public decimal OvertimeRateMultiplier { get; private set; }
    public int WorkingDaysPerMonth { get; private set; }
    public decimal StandardDailyHours { get; private set; }
    public int LeaveBalanceSnapshotDays { get; private set; }

    // Attendance aggregates
    public int TotalScheduledDays { get; private set; }
    public int TotalPresentDays { get; private set; }
    public int TotalAbsentDays { get; private set; }
    public int TotalLeaveDays { get; private set; }
    public int TotalOvertimeMinutes { get; private set; }
    public int LateOccurrenceCount { get; private set; }
    public int LatePenaltyUnits { get; private set; }

    // Monetary — earnings
    public decimal BaseSalary { get; private set; }
    public decimal TotalAllowances { get; private set; }
    public decimal NonTaxableAllowancesTotal { get; private set; }
    public decimal OvertimePay { get; private set; }
    public decimal GrossPay { get; private set; }

    // Monetary — deductions
    public decimal LeaveDeduction { get; private set; }
    public decimal LatePenaltyDeduction { get; private set; }
    public decimal SocialInsuranceEmployeeShare { get; private set; }
    public decimal SocialInsuranceEmployerShare { get; private set; }
    public decimal TaxableIncome { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalDeductions { get; private set; }

    // Monetary — net
    public decimal NetPay { get; private set; }

    // Audit
    public DateTime CalculatedAt { get; private set; }
    public string CalculatedBy { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    private PayrollRunDetail() { }

    public PayrollRunDetail(Guid employeeId, Guid contractVersionId, string calculatedBy)
    {
        EmployeeId = employeeId;
        ContractVersionId = contractVersionId;
        CalculatedBy = calculatedBy;
        CalculatedAt = DateTime.UtcNow;
        Status = PayrollRunDetailStatus.Calculated;
    }

    public void SetSkip(SkipReason reason)
    {
        Status = PayrollRunDetailStatus.Skipped;
        SkipReason = reason;
    }

    public void SetFailure(string message)
    {
        Status = PayrollRunDetailStatus.Failed;
        FailureMessage = message;
    }

    internal void SetPayrollRunId(Guid payrollRunId)
    {
        PayrollRunId = payrollRunId;
    }

    public void SetAttendanceAggregates(
        int totalScheduledDays,
        int totalPresentDays,
        int totalAbsentDays,
        int totalLeaveDays,
        int totalOvertimeMinutes,
        int lateOccurrenceCount,
        int latePenaltyUnits)
    {
        TotalScheduledDays = totalScheduledDays;
        TotalPresentDays = totalPresentDays;
        TotalAbsentDays = totalAbsentDays;
        TotalLeaveDays = totalLeaveDays;
        TotalOvertimeMinutes = totalOvertimeMinutes;
        LateOccurrenceCount = lateOccurrenceCount;
        LatePenaltyUnits = latePenaltyUnits;
    }

    public void SetSnapshotValues(
        Guid? taxBracketSetId,
        Guid? socialInsuranceConfigId,
        Guid? payrollPolicyId,
        decimal overtimeRateMultiplier,
        int workingDaysPerMonth,
        decimal standardDailyHours,
        int leaveBalanceSnapshotDays)
    {
        TaxBracketSetId = taxBracketSetId;
        SocialInsuranceConfigId = socialInsuranceConfigId;
        PayrollPolicyId = payrollPolicyId;
        OvertimeRateMultiplier = overtimeRateMultiplier;
        WorkingDaysPerMonth = workingDaysPerMonth;
        StandardDailyHours = standardDailyHours;
        LeaveBalanceSnapshotDays = leaveBalanceSnapshotDays;
    }

    public void SetEarnings(
        decimal baseSalary,
        decimal totalAllowances,
        decimal nonTaxableAllowancesTotal,
        decimal overtimePay)
    {
        BaseSalary = baseSalary;
        TotalAllowances = totalAllowances;
        NonTaxableAllowancesTotal = nonTaxableAllowancesTotal;
        OvertimePay = overtimePay;
        GrossPay = baseSalary + totalAllowances + overtimePay;
    }

    public void SetDeductions(
        decimal leaveDeduction,
        decimal latePenaltyDeduction,
        decimal socialInsuranceEmployeeShare,
        decimal socialInsuranceEmployerShare,
        decimal taxableIncome,
        decimal taxAmount)
    {
        LeaveDeduction = leaveDeduction;
        LatePenaltyDeduction = latePenaltyDeduction;
        SocialInsuranceEmployeeShare = socialInsuranceEmployeeShare;
        SocialInsuranceEmployerShare = socialInsuranceEmployerShare;
        TaxableIncome = taxableIncome;
        TaxAmount = taxAmount;

        TotalDeductions = socialInsuranceEmployeeShare + taxAmount + leaveDeduction + latePenaltyDeduction;
        NetPay = GrossPay - TotalDeductions;
    }

    public void SetNotes(string? notes) => Notes = notes;
}
