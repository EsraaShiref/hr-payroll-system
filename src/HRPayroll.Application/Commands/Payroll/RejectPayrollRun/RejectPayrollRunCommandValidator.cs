using FluentValidation;

namespace HRPayroll.Application.Commands.Payroll.RejectPayrollRun;

public class RejectPayrollRunCommandValidator : AbstractValidator<RejectPayrollRunCommand>
{
    public RejectPayrollRunCommandValidator()
    {
        RuleFor(x => x.PayrollRunId)
            .NotEmpty()
            .WithMessage("Payroll run ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Rejection reason is required and must not exceed 500 characters.");
    }
}
