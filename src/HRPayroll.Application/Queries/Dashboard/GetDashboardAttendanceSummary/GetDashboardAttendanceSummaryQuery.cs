using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using MediatR;

namespace HRPayroll.Application.Queries.Dashboard.GetDashboardAttendanceSummary;

public sealed record GetDashboardAttendanceSummaryQuery(
    DateOnly Date) : IRequest<ErrorOr<DashboardAttendanceSummaryDto>>;
