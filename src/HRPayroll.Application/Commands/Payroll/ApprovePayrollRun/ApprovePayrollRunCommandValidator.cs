using FluentValidation;

namespace HRPayroll.Application.Commands.Payroll.ApprovePayrollRun;

public class ApprovePayrollRunCommandValidator : AbstractValidator<ApprovePayrollRunCommand>
{
    public ApprovePayrollRunCommandValidator()
    {
        RuleFor(x => x.PayrollRunId)
            .NotEmpty()
            .WithMessage("Payroll run ID is required.");
    }
}
