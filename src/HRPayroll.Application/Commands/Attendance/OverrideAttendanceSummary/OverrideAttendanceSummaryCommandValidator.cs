using ErrorOr;
using FluentValidation;

namespace HRPayroll.Application.Commands.Attendance.OverrideAttendanceSummary;

public class OverrideAttendanceSummaryCommandValidator : AbstractValidator<OverrideAttendanceSummaryCommand>
{
    public OverrideAttendanceSummaryCommandValidator()
    {
        RuleFor(x => x.SummaryId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x).Must(x => x.OverridePunchIn.HasValue || x.OverridePunchOut.HasValue)
            .WithMessage("At least one override value (punch in or punch out) must be provided.");
    }
}
