using ErrorOr;
using HRPayroll.Application.DTOs.Attendance;
using MediatR;

namespace HRPayroll.Application.Queries.LeaveRequests.GetLeaveBalances;

public sealed record GetLeaveBalancesQuery(Guid EmployeeId) : IRequest<ErrorOr<List<LeaveBalanceDto>>>;
