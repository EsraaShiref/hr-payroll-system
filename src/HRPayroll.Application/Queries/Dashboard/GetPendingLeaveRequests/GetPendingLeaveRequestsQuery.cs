using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using MediatR;

namespace HRPayroll.Application.Queries.Dashboard.GetPendingLeaveRequests;

public sealed record GetPendingLeaveRequestsQuery(
    int PageSize = 10) : IRequest<ErrorOr<List<PendingLeaveRequestDto>>>;
