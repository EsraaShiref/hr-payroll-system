using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.ValueObjects;
using MediatR;

namespace HRPayroll.Application.Commands.Shifts.UpdateShift;

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, ErrorOr<Success>>
{
    private readonly IRepository<Shift> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateShiftCommandHandler(IRepository<Shift> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateShiftCommand command, CancellationToken ct)
    {
        var shift = await _repository.GetByIdAsync(command.Id, ct);
        if (shift == null)
            return Error.NotFound("Shift.NotFound", "Shift not found.");

        var startTime = TimeOnly.Parse(command.StartTime);
        var endTime = TimeOnly.Parse(command.EndTime);
        var workingDays = (WorkingDayFlags)command.WorkingDays;

        shift.Update(command.Name, startTime, endTime, workingDays);
        shift.SetDescription(command.Description);

        shift.ConfigureRules(
            command.GracePeriodMinutes,
            command.LateThresholdMinutes,
            command.EarlyDepartureThresholdMinutes,
            command.OvertimeThresholdMinutes,
            command.MinimumWorkMinutesForPresence,
            command.MaxBreakMinutes);

        _repository.Update(shift);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success;
    }
}
