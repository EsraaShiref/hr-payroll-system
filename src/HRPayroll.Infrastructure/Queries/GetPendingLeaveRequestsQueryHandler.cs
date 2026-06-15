using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.LeaveRequests.GetPendingLeaveRequests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetPendingLeaveRequestsQueryHandler
    : IRequestHandler<GetPendingLeaveRequestsQuery, ErrorOr<List<LeaveRequestDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPendingLeaveRequestsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<LeaveRequestDto>>> Handle(
        GetPendingLeaveRequestsQuery query, CancellationToken ct)
    {
        var q = _db.LeaveRequests
            .AsNoTracking()
            .Include(l => l.Employee)
            .Where(l => l.Status == Domain.Enums.LeaveRequestStatus.Pending && !l.IsDeleted);

        if (query.DepartmentId.HasValue)
            q = q.Where(l => l.Employee.DepartmentId == query.DepartmentId.Value);

        var result = await q
            .OrderBy(l => l.StartDate)
            .Select(l => new LeaveRequestDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FirstName + " " + l.Employee.LastName,
                LeaveType = l.LeaveType.ToString(),
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                Status = l.Status.ToString(),
                Reason = l.Reason,
                RejectionReason = l.RejectionReason,
                CreatedAt = l.CreatedAt,
            })
            .ToListAsync(ct);

        return result;
    }
}
