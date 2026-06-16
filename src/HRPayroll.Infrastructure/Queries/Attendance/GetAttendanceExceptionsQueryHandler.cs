using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Attendance.GetAttendanceExceptions;
using HRPayroll.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries.Attendance;

internal class GetAttendanceExceptionsQueryHandler
    : IRequestHandler<GetAttendanceExceptionsQuery, ErrorOr<List<AttendanceExceptionDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAttendanceExceptionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<List<AttendanceExceptionDto>>> Handle(
        GetAttendanceExceptionsQuery query, CancellationToken ct)
    {
        var summaries = _dbContext.AttendanceDailySummaries
            .AsNoTracking()
            .Include(s => s.Employee)
            .Where(s => !s.IsDeleted);

        if (query.FromDate.HasValue)
            summaries = summaries.Where(s => s.Date >= query.FromDate.Value);
        if (query.ToDate.HasValue)
            summaries = summaries.Where(s => s.Date <= query.ToDate.Value);
        if (query.EmployeeId is not null && Guid.TryParse(query.EmployeeId, out var empId))
            summaries = summaries.Where(s => s.EmployeeId == empId);

        var list = await summaries.OrderByDescending(s => s.Date).ToListAsync(ct);

        var exceptions = new List<AttendanceExceptionDto>();

        foreach (var summary in list)
        {
            var status = summary.Status;

            if (status == AttendanceSummaryStatus.PendingReview)
            {
                exceptions.Add(new AttendanceExceptionDto
                {
                    Id = summary.Id,
                    EmployeeName = $"{summary.Employee?.FirstName} {summary.Employee?.LastName}",
                    EmployeeCode = summary.Employee?.EmployeeCode?.Value ?? "",
                    Date = summary.Date,
                    ExceptionType = "PendingReview",
                    Severity = "Warning",
                    Details = summary.FirstPunchIn.HasValue && !summary.LastPunchOut.HasValue
                        ? "Punched in but no punch out recorded."
                        : "Punched out without matching punch in.",
                    SummaryId = summary.Id,
                    CanOverride = true,
                });
            }

            if (status == AttendanceSummaryStatus.AbsentUnexcused)
            {
                exceptions.Add(new AttendanceExceptionDto
                {
                    Id = summary.Id,
                    EmployeeName = $"{summary.Employee?.FirstName} {summary.Employee?.LastName}",
                    EmployeeCode = summary.Employee?.EmployeeCode?.Value ?? "",
                    Date = summary.Date,
                    ExceptionType = "UnexcusedAbsence",
                    Severity = "Error",
                    Details = "Employee was absent on a scheduled working day with no leave or holiday.",
                    SummaryId = summary.Id,
                    CanOverride = true,
                });
            }

            if (status == AttendanceSummaryStatus.Late)
            {
                exceptions.Add(new AttendanceExceptionDto
                {
                    Id = summary.Id,
                    EmployeeName = $"{summary.Employee?.FirstName} {summary.Employee?.LastName}",
                    EmployeeCode = summary.Employee?.EmployeeCode?.Value ?? "",
                    Date = summary.Date,
                    ExceptionType = "LateArrival",
                    Severity = "Warning",
                    Details = $"Employee arrived late. Worked {summary.NetWorkedMinutes} minutes.",
                    SummaryId = summary.Id,
                    CanOverride = false,
                });
            }

            if (status == AttendanceSummaryStatus.EarlyDeparture)
            {
                exceptions.Add(new AttendanceExceptionDto
                {
                    Id = summary.Id,
                    EmployeeName = $"{summary.Employee?.FirstName} {summary.Employee?.LastName}",
                    EmployeeCode = summary.Employee?.EmployeeCode?.Value ?? "",
                    Date = summary.Date,
                    ExceptionType = "EarlyDeparture",
                    Severity = "Warning",
                    Details = "Employee left before scheduled end time.",
                    SummaryId = summary.Id,
                    CanOverride = false,
                });
            }
        }

        // Apply exception type filter in memory (after derived status computation)
        // TODO: Push exception-type filter to DB-side WHERE clause using stored fields
        // (IsUnexcusedAbsence for AbsentUnexcused, LateMinutes > 0 for Late, etc.)
        // Current memory-filter is acceptable for early deployment volumes.
        if (!string.IsNullOrWhiteSpace(query.ExceptionType))
        {
            var filter = query.ExceptionType.Trim();
            exceptions = exceptions.Where(e =>
                e.ExceptionType.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return exceptions;
    }
}
