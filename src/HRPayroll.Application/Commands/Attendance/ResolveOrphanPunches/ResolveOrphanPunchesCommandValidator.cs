using ErrorOr;
using FluentValidation;

namespace HRPayroll.Application.Commands.Attendance.ResolveOrphanPunches;

public class ResolveOrphanPunchesCommandValidator : AbstractValidator<ResolveOrphanPunchesCommand>
{
    public ResolveOrphanPunchesCommandValidator()
    {
        RuleFor(x => x.PunchId).NotEmpty();
    }
}
