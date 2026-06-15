using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Attendance.GetMonthlyAttendanceSummary;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetMonthlyAttendanceSummaryQueryHandler
    : IRequestHandler<GetMonthlyAttendanceSummaryQuery, ErrorOr<List<AttendanceSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetMonthlyAttendanceSummaryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<AttendanceSummaryDto>>> Handle(
        GetMonthlyAttendanceSummaryQuery query, CancellationToken ct)
    {
        var from = new DateOnly(query.Year, query.Month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var q = _db.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.Date >= from && a.Date <= to && !a.IsDeleted);

        if (query.DepartmentId.HasValue)
            q = q.Where(a => a.Employee.DepartmentId == query.DepartmentId.Value);

        var summary = await q
            .GroupBy(a => new { a.EmployeeId, a.Employee.FirstName, a.Employee.LastName })
            .Select(g => new AttendanceSummaryDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = g.Key.FirstName + " " + g.Key.LastName,
                Year = query.Year,
                Month = query.Month,
                PresentDays = g.Count(a => a.Status == Domain.Enums.AttendanceStatus.Present),
                AbsentDays = g.Count(a => a.Status == Domain.Enums.AttendanceStatus.Absent),
                LateDays = g.Count(a => a.Status == Domain.Enums.AttendanceStatus.Late),
                HalfDays = g.Count(a => a.Status == Domain.Enums.AttendanceStatus.HalfDay),
                LeaveDays = g.Count(a => a.Status == Domain.Enums.AttendanceStatus.OnLeave),
                TotalWorkedMinutes = g.Sum(a => a.WorkedMinutes),
            })
            .ToListAsync(ct);

        return summary;
    }
}
