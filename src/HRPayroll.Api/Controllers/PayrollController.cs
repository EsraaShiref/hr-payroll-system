using HRPayroll.Application.Commands.Payroll.ApprovePayrollRun;
using HRPayroll.Application.Commands.Payroll.FinalizePayrollRun;
using HRPayroll.Application.Commands.Payroll.PatchRunPayroll;
using HRPayroll.Application.Commands.Payroll.RejectPayrollRun;
using HRPayroll.Application.Commands.Payroll.RunPayroll;
using HRPayroll.Application.Common.Security;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunDetail;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunsList;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunStatus;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

[Authorize(Policy = PayrollPolicies.PayrollManage)]
public class PayrollController : ApiController
{
    private readonly IMediator _mediator;

    public PayrollController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunPayrollRequest request, CancellationToken ct)
    {
        var command = new RunPayrollCommand(request.Year, request.Month, request.Notes);
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetPayrollRunsListQuery(page, pageSize), ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollRunStatusQuery(id), ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollRunSummaryQuery(id), ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}/details/{employeeId:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, Guid employeeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollRunDetailQuery(id, employeeId), ct);
        return OkOrError(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApprovePayrollRunCommand(id), ct);
        return OkOrError(result);
    }

    [HttpPost("{id:guid}/finalize")]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new FinalizePayrollRunCommand(id), ct);
        return OkOrError(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectPayrollRunRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectPayrollRunCommand(id, request.Reason), ct);
        return OkOrError(result);
    }

    [HttpPost("patch")]
    public async Task<IActionResult> Patch([FromBody] PatchRunPayrollRequest request, CancellationToken ct)
    {
        var command = new PatchRunPayrollCommand(request.OriginalRunId, request.EmployeeIds);
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }
}

public record RunPayrollRequest(int Year, int Month, string? Notes);

public record RejectPayrollRunRequest(string Reason);

public record PatchRunPayrollRequest(Guid OriginalRunId, List<Guid> EmployeeIds);
