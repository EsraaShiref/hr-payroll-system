using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Services;

public record PunchPairingResult
{
    public TimeOnly? FirstPunchIn { get; init; }
    public TimeOnly? LastPunchOut { get; init; }
    public int TotalBreakMinutes { get; init; }
    public List<AttendancePunch> OrphanPunches { get; init; } = new();
    public bool HasOrphans => OrphanPunches.Count > 0;
}
