using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.ClockIn;

public class ClockInCommandHandler : IRequestHandler<ClockInCommand, ErrorOr<Guid>>
{
    private readonly IAttendanceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ClockInCommandHandler(IAttendanceRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(ClockInCommand command, CancellationToken ct)
    {
        var existing = await _repository.GetByEmployeeAndDateAsync(command.EmployeeId, command.Date, ct);
        AttendanceRecord record;

        if (existing != null)
        {
            existing.ClockInRecord(command.Time);
            record = existing;
        }
        else
        {
            record = new AttendanceRecord(command.EmployeeId, command.Date);
            record.ClockInRecord(command.Time);
            _repository.Add(record);
        }

        record.ClearDomainEvents();
        await _unitOfWork.SaveChangesAsync(ct);
        return record.Id;
    }
}
