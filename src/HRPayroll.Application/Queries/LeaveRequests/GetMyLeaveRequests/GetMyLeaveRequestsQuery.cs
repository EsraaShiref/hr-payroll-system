using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using MediatR;

namespace HRPayroll.Application.Queries.LeaveRequests.GetMyLeaveRequests;

public sealed record GetMyLeaveRequestsQuery(
    int? Year,
    string? Status) : IRequest<ErrorOr<List<LeaveRequestDto>>>;
