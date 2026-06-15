using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.ClockOut;

public class ClockOutCommandHandler : IRequestHandler<ClockOutCommand, ErrorOr<Success>>
{
    private readonly IAttendanceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ClockOutCommandHandler(IAttendanceRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(ClockOutCommand command, CancellationToken ct)
    {
        var record = await _repository.GetByEmployeeAndDateAsync(command.EmployeeId, command.Date, ct);
        if (record == null)
            return Error.NotFound("Attendance.NotFound", "No attendance record found for this date.");

        record.SetBreakDuration(command.BreakDurationMinutes);
        record.ClockOutRecord(command.Time);
        record.ClearDomainEvents();

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
