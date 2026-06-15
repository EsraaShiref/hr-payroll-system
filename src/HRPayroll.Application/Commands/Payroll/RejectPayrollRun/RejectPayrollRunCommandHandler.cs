using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.RejectPayrollRun;

public class RejectPayrollRunCommandHandler : IRequestHandler<RejectPayrollRunCommand, ErrorOr<Success>>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectPayrollRunCommandHandler(IPayrollRepository payrollRepository, IUnitOfWork unitOfWork)
    {
        _payrollRepository = payrollRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(RejectPayrollRunCommand command, CancellationToken ct)
    {
        var run = await _payrollRepository.GetRunWithDetailsAsync(command.PayrollRunId, ct);
        if (run == null)
            return Error.NotFound("PayrollRun.NotFound", "Payroll run not found.");

        run.Reject("system", command.Reason);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
