using FluentValidation;

namespace HRPayroll.Application.Commands.Shifts.UpdateShift;

public class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
{
    public UpdateShiftCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.WorkingDays).GreaterThan(0);
    }
}
