using HRPayroll.Application.Queries.Positions.GetPositionsList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

[Authorize]
public class PositionsController : ApiController
{
    private readonly IMediator _mediator;

    public PositionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPositionsListQuery(), ct);
        return OkOrError(result);
    }
}
