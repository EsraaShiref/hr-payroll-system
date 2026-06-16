using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Holidays.DeleteHoliday;

public class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, ErrorOr<Success>>
{
    private readonly IHolidayRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHolidayCommandHandler(IHolidayRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteHolidayCommand command, CancellationToken ct)
    {
        var holiday = await _repository.GetByIdAsync(command.Id, ct);
        if (holiday == null)
            return Error.NotFound("Holiday.NotFound", "Holiday not found.");

        _repository.Remove(holiday);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success;
    }
}
