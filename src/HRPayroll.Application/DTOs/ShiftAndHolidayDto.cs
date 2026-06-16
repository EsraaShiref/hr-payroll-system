namespace HRPayroll.Application.DTOs;

public record ShiftDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string StartTime { get; init; } = "08:00";
    public string EndTime { get; init; } = "17:00";
    public int GracePeriodMinutes { get; init; }
    public int LateThresholdMinutes { get; init; }
    public int EarlyDepartureThresholdMinutes { get; init; }
    public int OvertimeThresholdMinutes { get; init; }
    public int MinimumWorkMinutesForPresence { get; init; }
    public int MaxBreakMinutes { get; init; }
    public int WorkingDays { get; init; }
}

public record HolidayDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public bool IsRecurringYearly { get; init; }
}
