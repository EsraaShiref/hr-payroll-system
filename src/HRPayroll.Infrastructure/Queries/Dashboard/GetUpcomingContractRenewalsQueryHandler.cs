using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Dashboard.GetUpcomingContractRenewals;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Dashboard;

internal sealed class GetUpcomingContractRenewalsQueryHandler
    : IRequestHandler<GetUpcomingContractRenewalsQuery, ErrorOr<List<UpcomingContractRenewalDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetUpcomingContractRenewalsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<UpcomingContractRenewalDto>>> Handle(
        GetUpcomingContractRenewalsQuery query, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(query.DaysAhead);

        var renewals = await _db.Contracts
            .AsNoTracking()
            .Include(c => c.Employee)
            .Where(c => c.Status == Domain.Enums.ContractStatus.Active
                && c.ExpiryDate.HasValue
                && c.ExpiryDate >= today
                && c.ExpiryDate <= cutoff
                && !c.IsDeleted)
            .OrderBy(c => c.ExpiryDate)
            .ToListAsync(ct);

        return renewals.Select(c => new UpcomingContractRenewalDto
        {
            ContractId = c.Id,
            EmployeeId = c.EmployeeId,
            EmployeeName = $"{c.Employee?.FirstName} {c.Employee?.LastName}",
            ContractType = c.ContractType.ToString(),
            ExpiryDate = c.ExpiryDate!.Value,
            DaysRemaining = c.ExpiryDate!.Value.DayNumber - today.DayNumber,
        }).ToList();
    }
}
