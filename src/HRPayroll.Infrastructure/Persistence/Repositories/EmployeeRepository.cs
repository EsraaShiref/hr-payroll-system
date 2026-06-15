using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);

    public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(e => e.EmployeeCode.Value == employeeCode && !e.IsDeleted, ct);

    public async Task<Employee?> GetWithContractsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(e => e.Contracts.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Versions.Where(v => !v.IsDeleted))
                    .ThenInclude(v => v.AllowanceAssignments.Where(aa => !aa.IsDeleted))
                        .ThenInclude(aa => aa.Allowance)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);

    public async Task<Employee?> GetWithDepartmentAndShiftAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(e => e.Department)
                .ThenInclude(d => d!.DefaultShift)
            .Include(e => e.Shift)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);

    public async Task<List<Employee>> GetAllActiveAsync(CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active && !e.IsDeleted)
            .ToListAsync(ct);

    public async Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, CancellationToken ct = default)
        => !await DbSet.AnyAsync(e => e.EmployeeCode.Value == employeeCode && !e.IsDeleted, ct);

    public async Task<bool> IsNationalIdUniqueAsync(string nationalId, CancellationToken ct = default)
        => !await DbSet.AnyAsync(e => e.NationalId == nationalId && !e.IsDeleted, ct);
}
