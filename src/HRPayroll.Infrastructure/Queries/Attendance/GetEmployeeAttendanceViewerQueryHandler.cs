using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Attendance.GetEmployeeAttendanceViewer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Attendance;

internal sealed class GetEmployeeAttendanceViewerQueryHandler
    : IRequestHandler<GetEmployeeAttendanceViewerQuery, ErrorOr<AttendanceViewerResult>>
{
    private readonly IApplicationDbContext _db;

    public GetEmployeeAttendanceViewerQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<AttendanceViewerResult>> Handle(
        GetEmployeeAttendanceViewerQuery query, CancellationToken ct)
    {
        var start = new DateOnly(query.Year, query.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var summaries = await _db.AttendanceDailySummaries
            .AsNoTracking()
            .Include(s => s.Employee)
            .Where(s => s.EmployeeId == query.EmployeeId && s.Date >= start && s.Date <= end && !s.IsDeleted)
            .OrderBy(s => s.Date)
            .ToListAsync(ct);

        var employee = summaries.FirstOrDefault()?.Employee;
        var days = summaries.Select(s => new AttendanceViewerItemDto
        {
            Date = s.Date,
            Status = s.Status.ToString(),
            FirstPunchIn = s.FirstPunchIn?.ToString("HH:mm"),
            LastPunchOut = s.LastPunchOut?.ToString("HH:mm"),
            Id = s.Id,
            NetWorkedMinutes = s.NetWorkedMinutes,
            LateMinutes = s.LateMinutes,
            EarlyDepartureMinutes = s.EarlyDepartureMinutes,
            OvertimeMinutes = s.OvertimeMinutes,
            IsOnLeave = s.IsOnLeave,
            IsHoliday = s.IsHoliday,
            Notes = s.Notes,
        }).ToList();

        var presentDays = days.Count(d => d.Status is "OnTime" or "Late" or "EarlyDeparture" or "PendingReview");
        var summary = new AttendanceViewerSummaryDto
        {
            TotalPresentDays = presentDays,
            TotalLateOccurrences = days.Count(d => d.LateMinutes > 0),
            TotalAbsentDays = days.Count(d => d.Status == "AbsentUnexcused"),
            TotalLeaveDays = days.Count(d => d.IsOnLeave),
            TotalHolidayDays = days.Count(d => d.IsHoliday),
            TotalOvertimeHours = Math.Round(days.Sum(d => d.OvertimeMinutes) / 60m, 1),
            TotalWorkedMinutes = days.Sum(d => d.NetWorkedMinutes),
        };

        return new AttendanceViewerResult
        {
            EmployeeId = query.EmployeeId,
            EmployeeName = employee is not null ? $"{employee.FirstName} {employee.LastName}" : "",
            Year = query.Year,
            Month = query.Month,
            Days = days,
            Summary = summary,
        };
    }
}
