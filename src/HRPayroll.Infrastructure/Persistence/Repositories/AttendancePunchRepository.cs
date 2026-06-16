using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class AttendancePunchRepository : Repository<AttendancePunch>, IAttendancePunchRepository
{
    public AttendancePunchRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public new async Task<AttendancePunch?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

    public async Task<List<AttendancePunch>> GetUnprocessedByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Where(p => !p.IsProcessed && !p.IsDeleted
                     && p.TimestampUtc >= from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                     && p.TimestampUtc <= to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
            .OrderBy(p => p.EmployeeId)
            .ThenBy(p => p.TimestampUtc)
            .ToListAsync(ct);

    public async Task<List<AttendancePunch>> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Where(p => p.EmployeeId == employeeId && !p.IsDeleted
                     && DateOnly.FromDateTime(p.TimestampUtc) == date)
            .OrderBy(p => p.TimestampUtc)
            .ToListAsync(ct);

    public async Task<bool> ExistsDuplicateAsync(Guid employeeId, DateTime timestampUtc, PunchType type, string? deviceId, CancellationToken ct = default)
        => await DbSet.AnyAsync(p =>
            p.EmployeeId == employeeId
            && p.TimestampUtc == timestampUtc
            && p.Type == type
            && p.DeviceId == deviceId
            && !p.IsDeleted, ct);

    public void AddRange(IEnumerable<AttendancePunch> punches) => DbSet.AddRange(punches);
}
