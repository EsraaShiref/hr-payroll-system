using FluentValidation;

namespace HRPayroll.Application.Commands.Shifts.CreateShift;

public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
{
    public CreateShiftCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.WorkingDays).GreaterThan(0);
    }
}
