using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class ContractRepository : Repository<Contract>, IContractRepository
{
    public ContractRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

    public async Task<Contract?> GetWithVersionsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(c => c.Versions.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.AllowanceAssignments.Where(aa => !aa.IsDeleted))
                    .ThenInclude(aa => aa.Allowance)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

    public async Task<Contract?> GetActiveForEmployeeAsync(Guid employeeId, CancellationToken ct = default)
        => await DbSet
            .Include(c => c.Versions.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.AllowanceAssignments.Where(aa => !aa.IsDeleted))
                    .ThenInclude(aa => aa.Allowance)
            .FirstOrDefaultAsync(
                c => c.EmployeeId == employeeId
                  && c.Status == ContractStatus.Active
                  && !c.IsDeleted, ct);

    public async Task<ContractVersion?> GetEffectiveVersionAsync(Guid employeeId, DateOnly effectiveDate, CancellationToken ct = default)
    {
        var contract = await DbSet
            .Include(c => c.Versions.Where(v => !v.IsDeleted && v.EffectiveFrom <= effectiveDate
                && (!v.EffectiveTo.HasValue || v.EffectiveTo > effectiveDate)))
                .ThenInclude(v => v.AllowanceAssignments.Where(aa => !aa.IsDeleted))
                    .ThenInclude(aa => aa.Allowance)
            .FirstOrDefaultAsync(
                c => c.EmployeeId == employeeId
                  && c.Status == ContractStatus.Active
                  && !c.IsDeleted, ct);

        return contract?.GetVersionEffectiveOn(effectiveDate);
    }
}
