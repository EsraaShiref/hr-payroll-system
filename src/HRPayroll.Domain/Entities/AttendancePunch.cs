using HRPayroll.Domain.Enums;

namespace HRPayroll.Domain.Entities;

public class AttendancePunch : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateTime TimestampUtc { get; private set; }
    public PunchType Type { get; private set; }
    public PunchSource Source { get; private set; }
    public bool IsProcessed { get; private set; }
    public string? DeviceId { get; private set; }
    public string? Notes { get; private set; }

    private AttendancePunch() { }

    public AttendancePunch(Guid employeeId, DateTime timestampUtc, PunchType type, PunchSource source, string? deviceId = null)
    {
        EmployeeId = employeeId;
        TimestampUtc = timestampUtc;
        Type = type;
        Source = source;
        IsProcessed = false;
        DeviceId = deviceId;
    }

    public void MarkProcessed()
    {
        IsProcessed = true;
    }

    public DateOnly PunchDate => DateOnly.FromDateTime(TimestampUtc);
    public TimeOnly PunchTime => TimeOnly.FromDateTime(TimestampUtc);
}
