using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using MediatR;

namespace HRPayroll.Application.Queries.Attendance.GetAttendanceExceptions;

public sealed record GetAttendanceExceptionsQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? EmployeeId = null,
    string? ExceptionType = null) : IRequest<ErrorOr<List<AttendanceExceptionDto>>>;
