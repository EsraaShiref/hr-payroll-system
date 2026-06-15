using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.RunPayroll;

public class RunPayrollCommandHandler : IRequestHandler<RunPayrollCommand, ErrorOr<Guid>>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobScheduler _jobScheduler;

    public RunPayrollCommandHandler(
        IPayrollRepository payrollRepository,
        IUnitOfWork unitOfWork,
        IJobScheduler jobScheduler)
    {
        _payrollRepository = payrollRepository;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
    }

    public async Task<ErrorOr<Guid>> Handle(RunPayrollCommand command, CancellationToken ct)
    {
        if (await _payrollRepository.HasFinalizedRunForPeriodAsync(command.Year, command.Month, ct))
            return Error.Conflict("PayrollRun.PeriodAlreadyFinalized",
                $"Payroll for {command.Year}-{command.Month:D2} has already been finalized.");

        if (await _payrollRepository.HasActiveProcessingRunForPeriodAsync(command.Year, command.Month, ct))
            return Error.Conflict("PayrollRun.PeriodInProgress",
                $"A payroll run for {command.Year}-{command.Month:D2} is already in progress.");

        var run = new PayrollRun(command.Year, command.Month);

        run.StartProcessing("system");

        _payrollRepository.Add(run);
        await _unitOfWork.SaveChangesAsync(ct);

        _jobScheduler.EnqueuePayrollRun(run.Id);

        return run.Id;
    }
}
