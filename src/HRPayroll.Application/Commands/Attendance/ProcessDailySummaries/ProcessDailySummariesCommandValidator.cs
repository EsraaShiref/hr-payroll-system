using ErrorOr;
using FluentValidation;

namespace HRPayroll.Application.Commands.Attendance.ProcessDailySummaries;

public class ProcessDailySummariesCommandValidator : AbstractValidator<ProcessDailySummariesCommand>
{
    public ProcessDailySummariesCommandValidator()
    {
        RuleFor(x => x.FromDate).NotEmpty();
        RuleFor(x => x.ToDate).NotEmpty();
        RuleFor(x => x).Must(x => x.ToDate >= x.FromDate)
            .WithMessage("ToDate must be on or after FromDate.");
        RuleFor(x => x).Must(x => (x.ToDate.DayNumber - x.FromDate.DayNumber) <= 92)
            .WithMessage("Date range cannot exceed 92 days (approximately 3 months).");
    }
}
