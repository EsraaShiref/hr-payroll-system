using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Holidays.DeleteHoliday;

public sealed record DeleteHolidayCommand(Guid Id) : IRequest<ErrorOr<Success>>;
