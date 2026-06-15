using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class AttendanceRepository : Repository<AttendanceRecord>, IAttendanceRepository
{
    public AttendanceRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);

    public async Task<AttendanceRecord?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date && !a.IsDeleted, ct);

    public async Task<List<AttendanceRecord>> GetByEmployeeDateRangeAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.Date >= from && a.Date <= to && !a.IsDeleted)
            .OrderBy(a => a.Date)
            .ToListAsync(ct);

    public async Task<List<AttendanceRecord>> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.Date == date && !a.IsDeleted)
            .ToListAsync(ct);
}
