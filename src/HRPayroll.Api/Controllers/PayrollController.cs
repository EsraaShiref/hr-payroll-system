using HRPayroll.Application.Commands.Payroll.ApprovePayrollRun;
using HRPayroll.Application.Commands.Payroll.FinalizePayrollRun;
using HRPayroll.Application.Commands.Payroll.PatchRunPayroll;
using HRPayroll.Application.Commands.Payroll.RejectPayrollRun;
using HRPayroll.Application.Commands.Payroll.RunPayroll;
using HRPayroll.Application.Common.Security;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunDetail;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunsList;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunStatus;
using HRPayroll.Application.Queries.Payroll.GetPayrollRunSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Api.Controllers;

[Authorize(Policy = PayrollPolicies.PayrollManage)]
public class PayrollController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IPaymentFileExportService _paymentFileExport;
    private readonly IPayslipGeneratorService _payslipGenerator;
    private readonly IApplicationDbContext _dbContext;

    public PayrollController(
        IMediator mediator,
        IPaymentFileExportService paymentFileExport,
        IPayslipGeneratorService payslipGenerator,
        IApplicationDbContext dbContext)
    {
        _mediator = mediator;
        _paymentFileExport = paymentFileExport;
        _payslipGenerator = payslipGenerator;
        _dbContext = dbContext;
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

    [HttpGet("{id:guid}/export/csv")]
    public async Task<IActionResult> ExportCsv(Guid id, CancellationToken ct)
    {
        var details = await _dbContext.PayrollRunDetails
            .AsNoTracking()
            .Where(d => d.PayrollRunId == id && !d.IsDeleted
                && d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated)
            .Include(d => d.Employee)
            .ToListAsync(ct);

        if (details.Count == 0)
            return NotFound("No calculated payroll details found for this run.");

        var csvBytes = await _paymentFileExport.GenerateBankTransferCsvAsync(details, ct);
        return File(csvBytes, "text/csv", $"payroll-export-{id}.csv");
    }

    [HttpGet("{id:guid}/export/payslips")]
    public async Task<IActionResult> ExportPayslips(Guid id, CancellationToken ct)
    {
        var details = await _dbContext.PayrollRunDetails
            .AsNoTracking()
            .Where(d => d.PayrollRunId == id && !d.IsDeleted
                && d.Status == Domain.Enums.PayrollRunDetailStatus.Calculated)
            .Include(d => d.Employee)
            .ToListAsync(ct);

        if (details.Count == 0)
            return NotFound("No calculated payroll details found for this run.");

        // TODO: Generate ZIP archive of individual payslip PDFs
        // For now, generate a single PDF placeholder
        var firstDetail = details.First();
        var pdfBytes = await _payslipGenerator.GeneratePayslipPdfAsync(firstDetail, ct);
        return File(pdfBytes, "application/pdf", $"payslips-{id}.pdf");
    }
}

public record RunPayrollRequest(int Year, int Month, string? Notes);

public record RejectPayrollRunRequest(string Reason);

public record PatchRunPayrollRequest(Guid OriginalRunId, List<Guid> EmployeeIds);
