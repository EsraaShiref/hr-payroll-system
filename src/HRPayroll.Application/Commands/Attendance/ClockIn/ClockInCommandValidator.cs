using FluentValidation;

namespace HRPayroll.Application.Commands.Attendance.ClockIn;

public class ClockInCommandValidator : AbstractValidator<ClockInCommand>
{
    public ClockInCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Time).NotEmpty();
    }
}
