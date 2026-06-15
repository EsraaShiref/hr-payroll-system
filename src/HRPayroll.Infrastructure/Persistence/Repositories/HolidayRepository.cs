using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence.Repositories;

public class HolidayRepository : Repository<Holiday>, IHolidayRepository
{
    public HolidayRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<List<Holiday>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Where(h => h.Date >= from && h.Date <= to && !h.IsDeleted)
            .OrderBy(h => h.Date)
            .ToListAsync(ct);

    public async Task<Holiday?> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(h => h.Date == date && !h.IsDeleted, ct);
}
