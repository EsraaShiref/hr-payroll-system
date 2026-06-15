using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

public interface IPayrollRepository
{
    Task<PayrollRun?> GetByIdAsync(Guid payrollRunId, CancellationToken ct = default);
    Task<PayrollRun?> GetRunWithDetailsAsync(Guid payrollRunId, CancellationToken ct = default);
    Task<PayrollPolicy?> GetActivePayrollPolicyAsync(DateOnly effectiveDate, CancellationToken ct = default);
    Task<bool> HasFinalizedRunForPeriodAsync(int year, int month, CancellationToken ct = default);
    Task<bool> HasActiveProcessingRunForPeriodAsync(int year, int month, CancellationToken ct = default);
    void Add(PayrollRun run);
    Task<List<PayrollRun>> GetRunsListAsync(int page, int pageSize, CancellationToken ct = default);
    Task<int> GetRunsCountAsync(CancellationToken ct = default);
}
