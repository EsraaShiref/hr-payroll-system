using HRPayroll.Domain.Enums;
using HRPayroll.Domain.Events;
using HRPayroll.Domain.Exceptions;
using MediatR;

namespace HRPayroll.Domain.Entities;

public class AttendanceRecord : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateOnly Date { get; private set; }
    public TimeOnly? ClockIn { get; private set; }
    public TimeOnly? ClockOut { get; private set; }
    public int BreakDurationMinutes { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private AttendanceRecord() { }

    public AttendanceRecord(Guid employeeId, DateOnly date)
    {
        EmployeeId = employeeId;
        Date = date;
        Status = AttendanceStatus.Absent;
    }

    public void ClockInRecord(TimeOnly time)
    {
        if (ClockIn.HasValue)
            throw new InvalidAttendanceException("Already clocked in for this date.");

        ClockIn = time;
        Status = time > new TimeOnly(9, 15) ? AttendanceStatus.Late : AttendanceStatus.Present;
        _domainEvents.Add(new AttendanceRecordedEvent(Id, EmployeeId, Date, Status.ToString(), DateTime.UtcNow));
    }

    public void ClockOutRecord(TimeOnly time)
    {
        if (!ClockIn.HasValue)
            throw new InvalidAttendanceException("Cannot clock out without clocking in first.");
        if (ClockOut.HasValue)
            throw new InvalidAttendanceException("Already clocked out for this date.");

        ClockOut = time;
    }

    public void SetBreakDuration(int minutes)
    {
        if (minutes < 0)
            throw new InvalidAttendanceException("Break duration cannot be negative.");
        if (minutes > 180)
            throw new InvalidAttendanceException("Break duration cannot exceed 180 minutes.");
        BreakDurationMinutes = minutes;
    }

    public void MarkAbsent(string? reason = null)
    {
        Status = AttendanceStatus.Absent;
        Notes = reason;
    }

    public void MarkHalfDay(TimeOnly clockOutTime)
    {
        if (!ClockIn.HasValue)
            throw new InvalidAttendanceException("Must clock in before marking half-day.");

        ClockOut = clockOutTime;
        Status = AttendanceStatus.HalfDay;
    }

    public int WorkedMinutes =>
        ClockIn.HasValue && ClockOut.HasValue
            ? (int)(ClockOut.Value - ClockIn.Value).TotalMinutes - BreakDurationMinutes
            : 0;

    public void ClearDomainEvents() => _domainEvents.Clear();
}
