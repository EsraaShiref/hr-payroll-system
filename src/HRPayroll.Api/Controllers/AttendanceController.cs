using HRPayroll.Application.Commands.Attendance.ClockIn;
using HRPayroll.Application.Commands.Attendance.ClockOut;
using HRPayroll.Application.Common.Security;
using HRPayroll.Application.Queries.Attendance.GetAttendanceForEmployee;
using HRPayroll.Application.Queries.Attendance.GetMonthlyAttendanceSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

[Authorize]
public class AttendanceController : ApiController
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = "Admin,HR")]
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetForEmployee(
        Guid employeeId,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAttendanceForEmployeeQuery(employeeId, fromDate, toDate), ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR,Manager")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetMonthlySummary(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] Guid? departmentId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMonthlyAttendanceSummaryQuery(year, month, departmentId), ct);
        return OkOrError(result);
    }

    [HttpPost("clock-in")]
    public async Task<IActionResult> ClockIn([FromBody] ClockInCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [HttpPost("clock-out")]
    public async Task<IActionResult> ClockOut([FromBody] ClockOutCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }
}
