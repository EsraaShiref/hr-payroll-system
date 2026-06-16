using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class LeaveBalanceRepository : Repository<LeaveBalance>, ILeaveBalanceRepository
{
    public LeaveBalanceRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public new async Task<LeaveBalance?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);

    public async Task<LeaveBalance?> GetByEmployeeAndTypeYearAsync(Guid employeeId, LeaveType leaveType, int year, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(
            l => l.EmployeeId == employeeId && l.LeaveType == leaveType && l.Year == year && !l.IsDeleted, ct);

    public async Task<List<LeaveBalance>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId && !l.IsDeleted)
            .ToListAsync(ct);
}
