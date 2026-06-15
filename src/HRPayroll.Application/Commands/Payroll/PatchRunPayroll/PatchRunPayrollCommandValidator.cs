using FluentValidation;

namespace HRPayroll.Application.Commands.Payroll.PatchRunPayroll;

public class PatchRunPayrollCommandValidator : AbstractValidator<PatchRunPayrollCommand>
{
    public PatchRunPayrollCommandValidator()
    {
        RuleFor(x => x.OriginalRunId)
            .NotEmpty()
            .WithMessage("Original payroll run ID is required.");

        RuleFor(x => x.EmployeeIds)
            .NotEmpty()
            .WithMessage("At least one employee ID is required.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Employee IDs must not be empty.");
    }
}
