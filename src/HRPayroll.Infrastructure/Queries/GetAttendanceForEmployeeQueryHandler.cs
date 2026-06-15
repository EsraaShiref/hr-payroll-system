using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Attendance.GetAttendanceForEmployee;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetAttendanceForEmployeeQueryHandler
    : IRequestHandler<GetAttendanceForEmployeeQuery, ErrorOr<List<AttendanceRecordDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAttendanceForEmployeeQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<AttendanceRecordDto>>> Handle(
        GetAttendanceForEmployeeQuery query, CancellationToken ct)
    {
        var records = await _db.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == query.EmployeeId
                     && a.Date >= query.FromDate
                     && a.Date <= query.ToDate
                     && !a.IsDeleted)
            .OrderBy(a => a.Date)
            .Select(a => new AttendanceRecordDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                Date = a.Date,
                ClockIn = a.ClockIn,
                ClockOut = a.ClockOut,
                BreakDurationMinutes = a.BreakDurationMinutes,
                WorkedMinutes = a.WorkedMinutes,
                Status = a.Status.ToString(),
                Notes = a.Notes,
            })
            .ToListAsync(ct);

        return records;
    }
}
