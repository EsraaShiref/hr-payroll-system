using HRPayroll.Application.Commands.LeaveRequests.Approve;
using HRPayroll.Application.Commands.LeaveRequests.Cancel;
using HRPayroll.Application.Commands.LeaveRequests.Reject;
using HRPayroll.Application.Commands.LeaveRequests.Submit;
using HRPayroll.Application.Common.Security;
using HRPayroll.Application.Queries.LeaveRequests.GetLeaveBalances;
using HRPayroll.Application.Queries.LeaveRequests.GetPendingLeaveRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

[Authorize]
public class LeaveRequestsController : ApiController
{
    private readonly IMediator _mediator;

    public LeaveRequestsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = "Admin,HR,Manager")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] Guid? departmentId = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPendingLeaveRequestsQuery(departmentId), ct);
        return OkOrError(result);
    }

    [HttpGet("balances/{employeeId:guid}")]
    public async Task<IActionResult> GetBalances(Guid employeeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLeaveBalancesQuery(employeeId), ct);
        return OkOrError(result);
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitLeaveRequestCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR,Manager")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveLeaveRequestCommand(id, request.ApprovedBy), ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR,Manager")]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectLeaveRequestCommand(id, request.RejectedBy, request.Reason), ct);
        return OkOrError(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelLeaveRequestCommand(id), ct);
        return OkOrError(result);
    }
}

public record ApproveRequest(Guid ApprovedBy);
public record RejectRequest(Guid RejectedBy, string Reason);
