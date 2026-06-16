using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class AttendanceSummaryRepository : Repository<AttendanceDailySummary>, IAttendanceSummaryRepository
{
    public AttendanceSummaryRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public new async Task<AttendanceDailySummary?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);

    public async Task<AttendanceDailySummary?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Date == date && !s.IsDeleted, ct);

    public async Task<List<AttendanceDailySummary>> GetByEmployeeDateRangeAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId && s.Date >= from && s.Date <= to && !s.IsDeleted)
            .OrderBy(s => s.Date)
            .ToListAsync(ct);

    public async Task<List<AttendanceDailySummary>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Include(s => s.Employee)
            .Where(s => s.Date >= from && s.Date <= to && !s.IsDeleted)
            .ToListAsync(ct);
}
