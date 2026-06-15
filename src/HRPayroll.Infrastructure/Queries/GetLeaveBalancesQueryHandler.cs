using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.LeaveRequests.GetLeaveBalances;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetLeaveBalancesQueryHandler
    : IRequestHandler<GetLeaveBalancesQuery, ErrorOr<List<LeaveBalanceDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetLeaveBalancesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<LeaveBalanceDto>>> Handle(
        GetLeaveBalancesQuery query, CancellationToken ct)
    {
        var balances = await _db.LeaveBalances
            .AsNoTracking()
            .Where(l => l.EmployeeId == query.EmployeeId && !l.IsDeleted)
            .ProjectToType<LeaveBalanceDto>()
            .ToListAsync(ct);

        return balances;
    }
}
