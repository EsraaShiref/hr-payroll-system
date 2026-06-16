using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Dashboard.GetDashboardAttendanceSummary;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Dashboard;

internal sealed class GetDashboardAttendanceSummaryQueryHandler
    : IRequestHandler<GetDashboardAttendanceSummaryQuery, ErrorOr<DashboardAttendanceSummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDashboardAttendanceSummaryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<DashboardAttendanceSummaryDto>> Handle(
        GetDashboardAttendanceSummaryQuery query, CancellationToken ct)
    {
        // Uses IX_AttendanceDailySummaries_EmployeeId_Date index via EmployeeId + Date filter
        var todaySummaries = _db.AttendanceDailySummaries
            .AsNoTracking()
            .Where(s => s.Date == query.Date && !s.IsDeleted);

        var counts = await todaySummaries
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Present = g.Count(s => s.Status != Domain.Enums.AttendanceSummaryStatus.AbsentUnexcused
                    && s.Status != Domain.Enums.AttendanceSummaryStatus.Holiday
                    && s.Status != Domain.Enums.AttendanceSummaryStatus.OnLeave),
                Absent = g.Count(s => s.IsUnexcusedAbsence),
                Late = g.Count(s => s.LateMinutes > 0),
                OnLeave = g.Count(s => s.IsOnLeave),
                Holiday = g.Count(s => s.IsHoliday),
                PendingReview = g.Count(s => s.Status == Domain.Enums.AttendanceSummaryStatus.PendingReview),
            })
            .FirstOrDefaultAsync(ct);

        var activeCount = await _db.Employees
            .AsNoTracking()
            .CountAsync(e => e.EmploymentStatus == Domain.Enums.EmploymentStatus.Active && !e.IsDeleted, ct);

        // Department breakdown
        // TODO: Remove redundant Include — navigation already covered by GroupBy FK
        var deptBreakdown = await todaySummaries
            .Include(s => s.Employee).ThenInclude(e => e.Department)
            .GroupBy(s => s.Employee.Department.Name)
            .Select(g => new DepartmentAttendanceDto
            {
                DepartmentName = g.Key,
                Total = g.Count(),
                Present = g.Count(s => s.Status != Domain.Enums.AttendanceSummaryStatus.AbsentUnexcused
                    && s.Status != Domain.Enums.AttendanceSummaryStatus.Holiday
                    && s.Status != Domain.Enums.AttendanceSummaryStatus.OnLeave),
                Absent = g.Count(s => s.IsUnexcusedAbsence),
                Late = g.Count(s => s.LateMinutes > 0),
                OnLeave = g.Count(s => s.IsOnLeave),
            })
            .ToListAsync(ct);

        return new DashboardAttendanceSummaryDto
        {
            TotalPresent = counts?.Present ?? 0,
            TotalAbsent = counts?.Absent ?? 0,
            TotalLate = counts?.Late ?? 0,
            TotalOnLeave = counts?.OnLeave ?? 0,
            TotalHoliday = counts?.Holiday ?? 0,
            TotalPendingReview = counts?.PendingReview ?? 0,
            TotalActiveEmployees = activeCount,
            DepartmentBreakdown = deptBreakdown,
        };
    }
}
