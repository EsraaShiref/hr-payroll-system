using HRPayroll.Application.Commands.Contracts.AddContractVersion;
using HRPayroll.Application.Commands.Contracts.AssignContract;
using HRPayroll.Application.Queries.Contracts.GetActiveContractForEmployee;
using HRPayroll.Application.Queries.Contracts.GetContractVersionForDate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

[Authorize(Roles = "Admin,HR,Manager")]
public class ContractsController : ApiController
{
    private readonly IMediator _mediator;

    public ContractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] AssignContractCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost("{id:guid}/versions")]
    public async Task<IActionResult> AddVersion(Guid id, [FromBody] AddContractVersionRequest request, CancellationToken ct)
    {
        var command = new AddContractVersionCommand(
            id,
            request.NewBaseSalaryAmount,
            request.NewBaseSalaryCurrency,
            request.EffectiveFrom,
            request.TaxBracketSetId,
            request.SocialInsuranceConfigId,
            request.AllowanceAssignments);

        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [Authorize(Policy = "EmployeeReadAccess")]
    [HttpGet("active/{employeeId:guid}")]
    public async Task<IActionResult> GetActiveForEmployee(Guid employeeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveContractForEmployeeQuery(employeeId), ct);
        return OkOrError(result);
    }

    [Authorize(Policy = "EmployeeReadAccess")]
    [HttpGet("version/{employeeId:guid}")]
    public async Task<IActionResult> GetVersionForDate(Guid employeeId, [FromQuery] DateOnly effectiveDate, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractVersionForDateQuery(employeeId, effectiveDate), ct);
        return OkOrError(result);
    }
}

public record AddContractVersionRequest(
    decimal NewBaseSalaryAmount,
    string NewBaseSalaryCurrency,
    DateOnly EffectiveFrom,
    Guid? TaxBracketSetId,
    Guid? SocialInsuranceConfigId,
    List<AllowanceAssignmentInput>? AllowanceAssignments);
