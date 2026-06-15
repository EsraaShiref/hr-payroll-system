using FluentValidation;

namespace HRPayroll.Application.Commands.Payroll.RunPayroll;

public class RunPayrollCommandValidator : AbstractValidator<RunPayrollCommand>
{
    public RunPayrollCommandValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Year must be between 2000 and 2100.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Month must be between 1 and 12.");
    }
}
