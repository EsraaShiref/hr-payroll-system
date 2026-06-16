using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Holidays.CreateHoliday;

public sealed record CreateHolidayCommand(
    string Name,
    DateOnly Date,
    bool IsRecurringYearly) : IRequest<ErrorOr<Guid>>;
