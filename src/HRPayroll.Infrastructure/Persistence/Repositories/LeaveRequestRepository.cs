using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);

    public async Task<List<LeaveRequest>> GetPendingByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Include(l => l.Employee)
            .Where(l => l.Status == Domain.Enums.LeaveRequestStatus.Pending
                     && l.Employee.DepartmentId == departmentId
                     && !l.IsDeleted)
            .OrderBy(l => l.StartDate)
            .ToListAsync(ct);

    public async Task<List<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);
}
