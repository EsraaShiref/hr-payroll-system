using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using MediatR;

namespace HRPayroll.Application.Queries.Attendance.GetEmployeeAttendanceViewer;

public sealed record GetEmployeeAttendanceViewerQuery(
    Guid EmployeeId,
    int Year,
    int Month) : IRequest<ErrorOr<AttendanceViewerResult>>;
