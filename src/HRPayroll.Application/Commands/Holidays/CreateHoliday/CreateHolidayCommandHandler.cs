using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using MediatR;

namespace HRPayroll.Application.Commands.Holidays.CreateHoliday;

public class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, ErrorOr<Guid>>
{
    private readonly IHolidayRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHolidayCommandHandler(IHolidayRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateHolidayCommand command, CancellationToken ct)
    {
        var existing = await _repository.GetByDateAsync(command.Date, ct);
        if (existing != null)
            return Error.Conflict("Holiday.DateConflict", $"A holiday already exists on {command.Date}.");

        var holiday = new Holiday(command.Name, command.Date, command.IsRecurringYearly);
        _repository.Add(holiday);
        await _unitOfWork.SaveChangesAsync(ct);

        return holiday.Id;
    }
}
