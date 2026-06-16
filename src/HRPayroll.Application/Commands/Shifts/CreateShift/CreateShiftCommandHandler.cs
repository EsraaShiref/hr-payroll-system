using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.ValueObjects;
using MediatR;

namespace HRPayroll.Application.Commands.Shifts.CreateShift;

public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, ErrorOr<Guid>>
{
    private readonly IRepository<Shift> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShiftCommandHandler(IRepository<Shift> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateShiftCommand command, CancellationToken ct)
    {
        var startTime = TimeOnly.Parse(command.StartTime);
        var endTime = TimeOnly.Parse(command.EndTime);
        var workingDays = (WorkingDayFlags)command.WorkingDays;

        var shift = new Shift(command.Name, startTime, endTime, workingDays);
        shift.SetDescription(command.Description);

        shift.ConfigureRules(
            command.GracePeriodMinutes,
            command.LateThresholdMinutes,
            command.EarlyDepartureThresholdMinutes,
            command.OvertimeThresholdMinutes,
            command.MinimumWorkMinutesForPresence,
            command.MaxBreakMinutes);

        _repository.Add(shift);
        await _unitOfWork.SaveChangesAsync(ct);

        return shift.Id;
    }
}
