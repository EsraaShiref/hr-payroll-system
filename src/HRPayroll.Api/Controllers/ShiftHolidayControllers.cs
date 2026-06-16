using HRPayroll.Application.Commands.Holidays.CreateHoliday;
using HRPayroll.Application.Commands.Holidays.DeleteHoliday;
using HRPayroll.Application.Commands.Shifts.CreateShift;
using HRPayroll.Application.Commands.Shifts.UpdateShift;
using HRPayroll.Application.Queries.Holidays.GetHolidaysList;
using HRPayroll.Application.Queries.Shifts.GetShiftById;
using HRPayroll.Application.Queries.Shifts.GetShiftsList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

[Authorize(Roles = "Admin,HR")]
public class ShiftsController : ApiController
{
    private readonly IMediator _mediator;

    public ShiftsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetShiftsListQuery(), ct);
        return OkOrError(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetShiftByIdQuery(id), ct);
        return OkOrError(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShiftCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShiftCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match command id.");

        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }
}

[Authorize(Roles = "Admin,HR")]
public class HolidaysController : ApiController
{
    private readonly IMediator _mediator;

    public HolidaysController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int? year, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHolidaysListQuery(year), ct);
        return OkOrError(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHolidayCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteHolidayCommand(id), ct);
        return OkOrError(result);
    }
}
