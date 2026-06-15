using ErrorOr;
using HRPayroll.Application.Commands.Employees.CreateEmployee;
using HRPayroll.Application.Commands.Employees.TerminateEmployee;
using HRPayroll.Application.Queries.Employees.GetEmployeeById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

[Authorize]
public class EmployeesController : ApiController
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = "EmployeeReadAccess")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery(id), ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost("{id:guid}/terminate")]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] TerminateEmployeeRequest request, CancellationToken ct)
    {
        var command = new TerminateEmployeeCommand(id, request.TerminationDate, request.Reason);
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }
}

public record TerminateEmployeeRequest(DateOnly TerminationDate, string Reason);
