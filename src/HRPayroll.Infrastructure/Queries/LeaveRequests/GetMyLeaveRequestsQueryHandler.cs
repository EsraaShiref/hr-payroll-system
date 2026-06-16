using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.LeaveRequests.GetMyLeaveRequests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.LeaveRequests;

internal sealed class GetMyLeaveRequestsQueryHandler
    : IRequestHandler<GetMyLeaveRequestsQuery, ErrorOr<List<LeaveRequestDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyLeaveRequestsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ErrorOr<List<LeaveRequestDto>>> Handle(
        GetMyLeaveRequestsQuery query, CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null) return Error.Unauthorized("User.NoEmployee", "No linked employee record.");

        var q = _db.LeaveRequests
            .AsNoTracking()
            .Include(lr => lr.Employee)
            .Where(lr => lr.EmployeeId == empId.Value && !lr.IsDeleted);

        if (query.Year.HasValue)
        {
            var start = new DateOnly(query.Year.Value, 1, 1);
            var end = new DateOnly(query.Year.Value, 12, 31);
            q = q.Where(lr => lr.StartDate >= start && lr.EndDate <= end);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (Enum.TryParse<Domain.Enums.LeaveRequestStatus>(query.Status, true, out var status))
                q = q.Where(lr => lr.Status == status);
        }

        var list = await q.OrderByDescending(lr => lr.StartDate).ToListAsync(ct);

        return list.Select(lr => new LeaveRequestDto
        {
            Id = lr.Id,
            EmployeeId = lr.EmployeeId,
            EmployeeName = $"{lr.Employee?.FirstName} {lr.Employee?.LastName}",
            LeaveType = lr.LeaveType.ToString(),
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            TotalDays = lr.TotalDays,
            Status = lr.Status.ToString(),
            Reason = lr.Reason,
            RejectionReason = lr.RejectionReason,
            CreatedAt = lr.CreatedAt,
        }).ToList();
    }
}
