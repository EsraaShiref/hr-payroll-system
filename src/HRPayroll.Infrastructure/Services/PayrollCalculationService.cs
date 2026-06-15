using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;

namespace HRPayroll.Infrastructure.Services;

/// <summary>
/// Pure computation — no DB calls, no async.
/// Formula steps map directly to the approved Module D design.
/// </summary>
public class PayrollCalculationService : IPayrollCalculationService
{
    public PayrollRunDetail Calculate(PayrollCalculationRequest request)
    {
        var summaries = request.AttendanceSummaries;

        // ──────────────────────────────────────────────
        // Step 1: Attendance aggregates
        // ──────────────────────────────────────────────
        var totalScheduledDays = summaries.Count;
        var totalPresentDays = summaries.Count(s => s.FirstPunchIn.HasValue);
        var totalAbsentDays = summaries.Count(s => s.IsUnexcusedAbsence);
        var totalLeaveDays = summaries.Count(s => s.IsOnLeave);
        var totalOvertimeMinutes = summaries.Sum(s => s.OvertimeMinutes);
        var lateOccurrenceCount = summaries.Count(s => s.LateMinutes > 0);

        var threshold = request.Policy.LateOccurrencesThreshold;
        var latePenaltyUnits = threshold > 0 ? lateOccurrenceCount / threshold : 0;

        var unpaidLeaveDays = Math.Max(0, totalLeaveDays - (request.LeaveBalance?.RemainingDays ?? 0));

        // ──────────────────────────────────────────────
        // Step 2: Rate derivation
        // ──────────────────────────────────────────────
        // DailyRate  = BaseSalary / WorkingDaysPerMonth
        // HourlyRate = DailyRate / StandardDailyHours
        var dailyRate = request.WorkingDaysPerMonth > 0
            ? request.BaseSalary / request.WorkingDaysPerMonth
            : 0;

        var hourlyRate = request.StandardDailyHours > 0
            ? dailyRate / request.StandardDailyHours
            : 0;

        var effectiveOvertimeRate = request.OvertimeRateMultiplier > 0
            ? request.OvertimeRateMultiplier
            : request.Policy.DefaultOvertimeRateMultiplier;

        // ──────────────────────────────────────────────
        // Step 3: Gross components (earnings)
        // ──────────────────────────────────────────────
        // GrossPay = BaseSalary + TotalAllowances + OvertimePay
        // OvertimePay = HourlyRate * OvertimeRateMultiplier * TotalOvertimeMinutes / 60
        var overtimePay = hourlyRate * effectiveOvertimeRate * totalOvertimeMinutes / 60m;
        var grossPay = request.BaseSalary + request.TotalAllowances + overtimePay;

        var detail = new PayrollRunDetail(request.EmployeeId, request.ContractVersionId, request.CalculatedBy);

        detail.SetAttendanceAggregates(
            totalScheduledDays,
            totalPresentDays,
            totalAbsentDays,
            totalLeaveDays,
            totalOvertimeMinutes,
            lateOccurrenceCount,
            latePenaltyUnits);

        detail.SetSnapshotValues(
            request.TaxBracketSet?.Id,
            request.SocialInsuranceConfig?.Id,
            request.Policy.Id,
            effectiveOvertimeRate,
            request.WorkingDaysPerMonth,
            request.StandardDailyHours,
            request.LeaveBalanceSnapshotDays);

        detail.SetEarnings(
            request.BaseSalary,
            request.TotalAllowances,
            request.NonTaxableAllowancesTotal,
            overtimePay);

        // ──────────────────────────────────────────────
        // Step 4: Statutory deductions
        // ──────────────────────────────────────────────
        var leaveDeduction = dailyRate * unpaidLeaveDays;
        var latePenaltyDeduction = dailyRate * latePenaltyUnits;

        var socialInsuranceEmployeeShare = request.SocialInsuranceConfig != null
            ? request.SocialInsuranceConfig.CalculateEmployeeContribution(grossPay)
            : 0;

        var socialInsuranceEmployerShare = request.SocialInsuranceConfig != null
            ? request.SocialInsuranceConfig.CalculateEmployerContribution(grossPay)
            : 0;

        // TaxableIncome = GrossPay - SocialInsuranceEmployeeShare - NonTaxableAllowancesTotal
        var taxableIncome = grossPay - socialInsuranceEmployeeShare - request.NonTaxableAllowancesTotal;

        var taxAmount = request.TaxBracketSet != null
            ? request.TaxBracketSet.CalculateTax(taxableIncome)
            : 0;

        detail.SetDeductions(
            leaveDeduction,
            latePenaltyDeduction,
            socialInsuranceEmployeeShare,
            socialInsuranceEmployerShare,
            taxableIncome,
            taxAmount);

        // ──────────────────────────────────────────────
        // Step 5: Net pay
        // NetPay = GrossPay - TotalDeductions
        // TotalDeductions = SocialInsuranceEmployeeShare + TaxAmount + LeaveDeduction + LatePenaltyDeduction
        // Calculated inside SetDeductions()
        // ──────────────────────────────────────────────

        return detail;
    }
}
