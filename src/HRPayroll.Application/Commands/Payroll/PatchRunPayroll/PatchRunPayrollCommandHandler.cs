using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.PatchRunPayroll;

public class PatchRunPayrollCommandHandler : IRequestHandler<PatchRunPayrollCommand, ErrorOr<Guid>>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobScheduler _jobScheduler;

    public PatchRunPayrollCommandHandler(
        IPayrollRepository payrollRepository,
        IUnitOfWork unitOfWork,
        IJobScheduler jobScheduler)
    {
        _payrollRepository = payrollRepository;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
    }

    public async Task<ErrorOr<Guid>> Handle(PatchRunPayrollCommand command, CancellationToken ct)
    {
        var originalRun = await _payrollRepository.GetRunWithDetailsAsync(command.OriginalRunId, ct);
        if (originalRun == null)
            return Error.NotFound("PayrollRun.NotFound", "Original payroll run not found.");

        if (await _payrollRepository.HasActiveProcessingRunForPeriodAsync(originalRun.Year, originalRun.Month, ct))
            return Error.Conflict("PayrollRun.PeriodInProgress",
                $"A payroll run for {originalRun.Year}-{originalRun.Month:D2} is already in progress.");

        var run = new PayrollRun(originalRun.Year, originalRun.Month);

        run.StartProcessing("system");

        _payrollRepository.Add(run);
        await _unitOfWork.SaveChangesAsync(ct);

        _jobScheduler.EnqueuePayrollRun(run.Id);

        return run.Id;
    }
}
