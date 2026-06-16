using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class PayrollRepository : Repository<PayrollRun>, IPayrollRepository
{
    public PayrollRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public new async Task<PayrollRun?> GetByIdAsync(Guid payrollRunId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(r => r.Id == payrollRunId && !r.IsDeleted, ct);

    public async Task<PayrollRun?> GetRunWithDetailsAsync(Guid payrollRunId, CancellationToken ct = default)
        => await DbSet
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == payrollRunId && !r.IsDeleted, ct);

    public async Task<PayrollPolicy?> GetActivePayrollPolicyAsync(DateOnly effectiveDate, CancellationToken ct = default)
        => await DbContext.Set<PayrollPolicy>()
            .FirstOrDefaultAsync(p => p.EffectiveFrom <= effectiveDate
                && (!p.EffectiveTo.HasValue || p.EffectiveTo.Value > effectiveDate)
                && !p.IsDeleted, ct);

    public async Task<bool> HasFinalizedRunForPeriodAsync(int year, int month, CancellationToken ct = default)
        => await DbSet.AnyAsync(r =>
            r.Year == year
            && r.Month == month
            && r.Status == Domain.Enums.PayrollRunStatus.Finalized
            && !r.IsDeleted, ct);

    public async Task<bool> HasActiveProcessingRunForPeriodAsync(int year, int month, CancellationToken ct = default)
        => await DbSet.AnyAsync(r =>
            r.Year == year
            && r.Month == month
            && (r.Status == Domain.Enums.PayrollRunStatus.Processing
                || r.Status == Domain.Enums.PayrollRunStatus.PendingReview
                || r.Status == Domain.Enums.PayrollRunStatus.Approved)
            && !r.IsDeleted, ct);

    public async Task<List<PayrollRun>> GetRunsListAsync(int page, int pageSize, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetRunsCountAsync(CancellationToken ct = default)
        => await DbSet.CountAsync(r => !r.IsDeleted, ct);
}
