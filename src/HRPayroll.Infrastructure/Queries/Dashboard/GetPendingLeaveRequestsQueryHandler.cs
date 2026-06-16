using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Dashboard.GetPendingLeaveRequests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Dashboard;

internal sealed class GetPendingLeaveRequestsQueryHandler
    : IRequestHandler<GetPendingLeaveRequestsQuery, ErrorOr<List<PendingLeaveRequestDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPendingLeaveRequestsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<PendingLeaveRequestDto>>> Handle(
        GetPendingLeaveRequestsQuery query, CancellationToken ct)
    {
        var pending = await _db.LeaveRequests
            .AsNoTracking()
            .Include(lr => lr.Employee)
            .Where(lr => lr.Status == Domain.Enums.LeaveRequestStatus.Pending && !lr.IsDeleted)
            .OrderByDescending(lr => lr.CreatedAt)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return pending.Select(lr => new PendingLeaveRequestDto
        {
            LeaveRequestId = lr.Id,
            EmployeeId = lr.EmployeeId,
            EmployeeName = $"{lr.Employee?.FirstName} {lr.Employee?.LastName}",
            LeaveType = lr.LeaveType.ToString(),
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            TotalDays = lr.TotalDays,
            Reason = lr.Reason,
            CreatedAt = lr.CreatedAt,
        }).ToList();
    }
}
