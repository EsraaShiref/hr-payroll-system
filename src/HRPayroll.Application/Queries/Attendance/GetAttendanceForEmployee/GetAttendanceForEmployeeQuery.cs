using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using MediatR;

namespace HRPayroll.Application.Queries.Attendance.GetAttendanceForEmployee;

public sealed record GetAttendanceForEmployeeQuery(
    Guid EmployeeId,
    DateOnly FromDate,
    DateOnly ToDate) : IRequest<ErrorOr<List<AttendanceRecordDto>>>;
