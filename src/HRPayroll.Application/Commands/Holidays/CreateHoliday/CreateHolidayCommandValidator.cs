using FluentValidation;

namespace HRPayroll.Application.Commands.Holidays.CreateHoliday;

public class CreateHolidayCommandValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Date).NotEmpty();
    }
}
