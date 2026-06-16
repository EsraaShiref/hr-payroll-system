using HRPayroll.Application.Commands.Attendance.UploadAttendanceFile;
using HRPayroll.Application.Commands.Attendance.ProcessDailySummaries;
using HRPayroll.Application.Commands.Attendance.OverrideAttendanceSummary;
using HRPayroll.Application.Commands.Attendance.ResolveOrphanPunches;
using HRPayroll.Application.Queries.Attendance.GetAttendanceExceptions;
using HRPayroll.Application.Queries.Attendance.GetEmployeeAttendanceViewer;
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
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var command = new UploadAttendanceFileCommand(file.FileName, ms.ToArray());
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost("process")]
    public async Task<IActionResult> ProcessDailySummaries(
        [FromBody] ProcessDailySummariesCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPut("summaries/{summaryId:guid}/override")]
    public async Task<IActionResult> OverrideSummary(
        Guid summaryId,
        [FromBody] OverrideAttendanceSummaryCommand command, CancellationToken ct)
    {
        if (summaryId != command.SummaryId)
            return BadRequest("Route summaryId does not match command SummaryId.");

        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost("resolve-orphans")]
    public async Task<IActionResult> ResolveOrphans(
        [FromBody] ResolveOrphanPunchesCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR,Manager")]
    [HttpGet("exceptions")]
    public async Task<IActionResult> GetExceptions(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? employeeId = null,
        [FromQuery] string? exceptionType = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAttendanceExceptionsQuery(fromDate, toDate, employeeId, exceptionType), ct);
        return OkOrError(result);
    }

    [Authorize(Roles = "Admin,HR,Manager")]
    [HttpGet("viewer/{employeeId:guid}")]
    public async Task<IActionResult> GetViewer(
        Guid employeeId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetEmployeeAttendanceViewerQuery(employeeId, year, month), ct);
        return OkOrError(result);
    }
}
