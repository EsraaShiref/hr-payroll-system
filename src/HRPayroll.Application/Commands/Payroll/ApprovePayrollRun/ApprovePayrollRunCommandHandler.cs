using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.ApprovePayrollRun;

public class ApprovePayrollRunCommandHandler : IRequestHandler<ApprovePayrollRunCommand, ErrorOr<Success>>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePayrollRunCommandHandler(IPayrollRepository payrollRepository, IUnitOfWork unitOfWork)
    {
        _payrollRepository = payrollRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(ApprovePayrollRunCommand command, CancellationToken ct)
    {
        var run = await _payrollRepository.GetRunWithDetailsAsync(command.PayrollRunId, ct);
        if (run == null)
            return Error.NotFound("PayrollRun.NotFound", "Payroll run not found.");

        run.Approve();
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
