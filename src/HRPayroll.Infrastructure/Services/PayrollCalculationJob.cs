using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using HRPayroll.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Services;

/// <summary>
/// DECISION POINT: Batch-load vs per-employee loop.
///
/// Current implementation uses per-employee data loading inside the loop for
/// clarity and simplicity. This avoids a complex in-memory matching layer but
/// means N+1 queries for large companies.
///
/// If profiling shows this is a bottleneck, refactor to batch-load all
/// ContractVersions, AttendanceDailySummaries, and LeaveBalances upfront
/// in bulk queries, then match in-memory by EmployeeId.
/// </summary>
public class PayrollCalculationJob : IPayrollCalculationJob
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPayrollCalculationService _calculationService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IPayrollRepository _payrollRepository;
    private readonly IContractRepository _contractRepository;

    public PayrollCalculationJob(
        ApplicationDbContext dbContext,
        IPayrollCalculationService calculationService,
        IEmployeeRepository employeeRepository,
        IAttendanceSummaryRepository summaryRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        IPayrollRepository payrollRepository,
        IContractRepository contractRepository)
    {
        _dbContext = dbContext;
        _calculationService = calculationService;
        _employeeRepository = employeeRepository;
        _summaryRepository = summaryRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _payrollRepository = payrollRepository;
        _contractRepository = contractRepository;
    }

    public async Task ProcessPayrollRunAsync(Guid payrollRunId)
    {
        var run = await _payrollRepository.GetRunWithDetailsAsync(payrollRunId);
        if (run == null)
            throw new InvalidOperationException($"PayrollRun {payrollRunId} not found.");

        var policy = await _payrollRepository.GetActivePayrollPolicyAsync(
            DateOnly.FromDateTime(DateTime.UtcNow));

        if (policy == null)
            throw new InvalidOperationException("No active payroll policy found.");

        var periodStart = new DateOnly(run.Year, run.Month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var employees = await _employeeRepository.GetAllActiveAsync();
        var employeeList = employees.ToList();

        foreach (var employee in employeeList)
        {
            // TODO: Batch-load optimization — see XML doc above
            try
            {
                var contractVersion = await _contractRepository.GetEffectiveVersionAsync(
                    employee.Id, periodStart);

                if (contractVersion == null)
                {
                    var skipDetail = new PayrollRunDetail(employee.Id, Guid.Empty, "system");
                    skipDetail.SetSkip(SkipReason.NoActiveContract);
                    run.AddDetail(skipDetail);
                    continue;
                }

                if (contractVersion.BaseSalary.Currency != policy.CurrencyCode)
                {
                    var skipDetail = new PayrollRunDetail(employee.Id, contractVersion.Id, "system");
                    skipDetail.SetSkip(SkipReason.CurrencyMismatch);
                    run.AddDetail(skipDetail);
                    continue;
                }

                var summaries = await _summaryRepository.GetByEmployeeDateRangeAsync(
                    employee.Id, periodStart, periodEnd);

                var pendingReview = summaries.Any(s => s.Status == AttendanceSummaryStatus.PendingReview);

                if (pendingReview)
                {
                    var skipDetail = new PayrollRunDetail(employee.Id, contractVersion.Id, "system");
                    skipDetail.SetSkip(SkipReason.PendingReviewAttendance);
                    run.AddDetail(skipDetail);
                    continue;
                }

                if (summaries.Count == 0)
                {
                    var skipDetail = new PayrollRunDetail(employee.Id, contractVersion.Id, "system");
                    skipDetail.SetSkip(SkipReason.MissingShift);
                    run.AddDetail(skipDetail);
                    continue;
                }

                var taxBracketSet = contractVersion.TaxBracketSetId.HasValue
                    ? await _dbContext.Set<TaxBracketSet>()
                        .FirstOrDefaultAsync(t => t.Id == contractVersion.TaxBracketSetId.Value)
                    : null;

                var socialInsuranceConfig = contractVersion.SocialInsuranceConfigId.HasValue
                    ? await _dbContext.Set<SocialInsuranceConfig>()
                        .FirstOrDefaultAsync(s => s.Id == contractVersion.SocialInsuranceConfigId.Value)
                    : null;

                var leaveBalance = await _leaveBalanceRepository
                    .GetByEmployeeAndTypeYearAsync(employee.Id, LeaveType.Annual, run.Year);

                var leaveBalanceSnapshotDays = (int)(leaveBalance?.RemainingDays ?? 0);

                var (totalAllowances, nonTaxableAllowances) = ComputeAllowances(contractVersion);

                var request = new PayrollCalculationRequest
                {
                    PayrollRunId = payrollRunId,
                    EmployeeId = employee.Id,
                    ContractVersionId = contractVersion.Id,
                    BaseSalary = contractVersion.BaseSalary.Amount,
                    OvertimeRateMultiplier = contractVersion.OvertimeRateMultiplier ?? policy.DefaultOvertimeRateMultiplier,
                    WorkingDaysPerMonth = policy.WorkingDaysPerMonth,
                    StandardDailyHours = policy.StandardDailyHours,
                    AttendanceSummaries = summaries,
                    TaxBracketSet = taxBracketSet,
                    SocialInsuranceConfig = socialInsuranceConfig,
                    Policy = policy,
                    LeaveBalance = leaveBalance,
                    LeaveBalanceSnapshotDays = leaveBalanceSnapshotDays,
                    TotalAllowances = totalAllowances,
                    NonTaxableAllowancesTotal = nonTaxableAllowances,
                    CalculatedBy = "system"
                };

                var detail = _calculationService.Calculate(request);
                run.AddDetail(detail);
            }
            catch (Exception ex)
            {
                var failedDetail = new PayrollRunDetail(employee.Id, Guid.Empty, "system");
                failedDetail.SetFailure(ex.Message);
                run.AddDetail(failedDetail);
            }
        }

        run.CompletePendingReview();

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Domain.Exceptions.PayrollConcurrencyConflictException(
                $"PayrollRun {payrollRunId} was modified by another process.");
        }
    }

    private static (decimal totalAllowances, decimal nonTaxableAllowances) ComputeAllowances(
        ContractVersion contractVersion)
    {
        var total = 0m;
        var nonTaxable = 0m;

        foreach (var assignment in contractVersion.AllowanceAssignments)
        {
            var value = assignment.ComputeValue(contractVersion.BaseSalary).Amount;
            total += value;

            if (assignment.Allowance?.Taxability == AllowanceTaxability.NonTaxable)
                nonTaxable += value;
        }

        return (total, nonTaxable);
    }
}
