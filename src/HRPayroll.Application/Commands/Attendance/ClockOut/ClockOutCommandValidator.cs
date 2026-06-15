using FluentValidation;

namespace HRPayroll.Application.Commands.Attendance.ClockOut;

public class ClockOutCommandValidator : AbstractValidator<ClockOutCommand>
{
    public ClockOutCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Time).NotEmpty();
        RuleFor(x => x.BreakDurationMinutes).InclusiveBetween(0, 180);
    }
}
