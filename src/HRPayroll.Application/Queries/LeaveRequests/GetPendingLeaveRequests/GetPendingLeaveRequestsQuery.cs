using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using MediatR;

namespace HRPayroll.Application.Queries.LeaveRequests.GetPendingLeaveRequests;

public sealed record GetPendingLeaveRequestsQuery(Guid? DepartmentId = null) : IRequest<ErrorOr<List<LeaveRequestDto>>>;
