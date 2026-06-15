using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using MediatR;

namespace HRPayroll.Application.Queries.Attendance.GetMonthlyAttendanceSummary;

public sealed record GetMonthlyAttendanceSummaryQuery(
    int Year,
    int Month,
    Guid? DepartmentId = null) : IRequest<ErrorOr<List<AttendanceSummaryDto>>>;
