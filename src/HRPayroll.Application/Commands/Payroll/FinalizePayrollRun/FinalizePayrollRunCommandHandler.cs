using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.FinalizePayrollRun;

public class FinalizePayrollRunCommandHandler : IRequestHandler<FinalizePayrollRunCommand, ErrorOr<Success>>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobScheduler _jobScheduler;

    public FinalizePayrollRunCommandHandler(
        IPayrollRepository payrollRepository,
        IUnitOfWork unitOfWork,
        IJobScheduler jobScheduler)
    {
        _payrollRepository = payrollRepository;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
    }

    public async Task<ErrorOr<Success>> Handle(FinalizePayrollRunCommand command, CancellationToken ct)
    {
        var run = await _payrollRepository.GetRunWithDetailsAsync(command.PayrollRunId, ct);
        if (run == null)
            return Error.NotFound("PayrollRun.NotFound", "Payroll run not found.");

        run.FinalizeRun();
        await _unitOfWork.SaveChangesAsync(ct);

        _jobScheduler.EnqueuePayslipGeneration(run.Id);

        return Result.Success;
    }
}
