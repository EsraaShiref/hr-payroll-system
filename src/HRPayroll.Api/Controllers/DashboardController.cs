using HRPayroll.Application.Queries.Dashboard.GetDashboardAttendanceSummary;
using HRPayroll.Application.Queries.Dashboard.GetHeadcountTrends;
using HRPayroll.Application.Queries.Dashboard.GetMonthlyPayrollBudgetSummary;
using HRPayroll.Application.Queries.Dashboard.GetPendingLeaveRequests;
using HRPayroll.Application.Queries.Dashboard.GetUpcomingContractRenewals;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

[Authorize(Roles = "Admin,HR")]
public class DashboardController : ApiController
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("attendance-summary")]
    public async Task<IActionResult> GetAttendanceSummary([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetDashboardAttendanceSummaryQuery(date ?? DateOnly.FromDateTime(DateTime.UtcNow)), ct);
        return OkOrError(result);
    }

    [HttpGet("pending-leave")]
    public async Task<IActionResult> GetPendingLeave(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPendingLeaveRequestsQuery(), ct);
        return OkOrError(result);
    }

    [HttpGet("payroll-budget")]
    public async Task<IActionResult> GetPayrollBudget(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMonthlyPayrollBudgetSummaryQuery(year, month), ct);
        return OkOrError(result);
    }

    [HttpGet("headcount-trend")]
    public async Task<IActionResult> GetHeadcountTrend(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHeadcountTrendsQuery(), ct);
        return OkOrError(result);
    }

    [HttpGet("contract-renewals")]
    public async Task<IActionResult> GetContractRenewals(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUpcomingContractRenewalsQuery(), ct);
        return OkOrError(result);
    }
}
