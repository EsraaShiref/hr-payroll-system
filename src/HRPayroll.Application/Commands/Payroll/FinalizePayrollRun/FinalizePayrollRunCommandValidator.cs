using FluentValidation;

namespace HRPayroll.Application.Commands.Payroll.FinalizePayrollRun;

public class FinalizePayrollRunCommandValidator : AbstractValidator<FinalizePayrollRunCommand>
{
    public FinalizePayrollRunCommandValidator()
    {
        RuleFor(x => x.PayrollRunId)
            .NotEmpty()
            .WithMessage("Payroll run ID is required.");
    }
}
