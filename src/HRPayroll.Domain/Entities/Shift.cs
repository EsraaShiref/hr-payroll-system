using HRPayroll.Domain.ValueObjects;

namespace HRPayroll.Domain.Entities;

public class Shift : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int GracePeriodMinutes { get; private set; }
    public int LateThresholdMinutes { get; private set; }
    public int EarlyDepartureThresholdMinutes { get; private set; }
    public int OvertimeThresholdMinutes { get; private set; }
    public int MinimumWorkMinutesForPresence { get; private set; }
    public int MaxBreakMinutes { get; private set; }
    public WorkingDayFlags WorkingDays { get; private set; }

    /// <summary>
    /// Hours between StartTime and EndTime, adjusted for shifts that cross midnight.
    /// </summary>
    public double ScheduledHours
    {
        get
        {
            var startMin = StartTime.Hour * 60 + StartTime.Minute;
            var endMin = EndTime.Hour * 60 + EndTime.Minute;
            if (EndTime <= StartTime)
            {
                return (1440 - startMin + endMin) / 60.0;
            }
            return (endMin - startMin) / 60.0;
        }
    }

    private Shift() { }

    public Shift(string name, TimeOnly startTime, TimeOnly endTime, WorkingDayFlags workingDays)
    {
        SetName(name);
        StartTime = startTime;
        EndTime = endTime;
        WorkingDays = workingDays;
        GracePeriodMinutes = 0;
        LateThresholdMinutes = 15;
        EarlyDepartureThresholdMinutes = 15;
        OvertimeThresholdMinutes = 60;
        var startMin = startTime.Hour * 60 + startTime.Minute;
        var endMin = endTime.Hour * 60 + endTime.Minute;
        MinimumWorkMinutesForPresence = 240; // 4-hour default, configurable via ConfigureRules
        MaxBreakMinutes = 60;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shift name is required.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Shift name cannot exceed 100 characters.", nameof(name));
        Name = name.Trim();
    }

    public void ConfigureRules(
        int gracePeriodMinutes,
        int lateThresholdMinutes,
        int earlyDepartureThresholdMinutes,
        int overtimeThresholdMinutes,
        int minimumWorkMinutes,
        int maxBreakMinutes)
    {
        if (gracePeriodMinutes < 0) throw new ArgumentOutOfRangeException(nameof(gracePeriodMinutes));
        if (lateThresholdMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(lateThresholdMinutes));
        if (earlyDepartureThresholdMinutes < 0) throw new ArgumentOutOfRangeException(nameof(earlyDepartureThresholdMinutes));
        if (overtimeThresholdMinutes < 0) throw new ArgumentOutOfRangeException(nameof(overtimeThresholdMinutes));
        if (minimumWorkMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(minimumWorkMinutes));
        if (maxBreakMinutes < 0) throw new ArgumentOutOfRangeException(nameof(maxBreakMinutes));

        GracePeriodMinutes = gracePeriodMinutes;
        LateThresholdMinutes = lateThresholdMinutes;
        EarlyDepartureThresholdMinutes = earlyDepartureThresholdMinutes;
        OvertimeThresholdMinutes = overtimeThresholdMinutes;
        MinimumWorkMinutesForPresence = minimumWorkMinutes;
        MaxBreakMinutes = maxBreakMinutes;
    }

    public bool IsWorkingDay(DayOfWeek day) => WorkingDays.HasFlag(day switch
    {
        DayOfWeek.Monday => WorkingDayFlags.Monday,
        DayOfWeek.Tuesday => WorkingDayFlags.Tuesday,
        DayOfWeek.Wednesday => WorkingDayFlags.Wednesday,
        DayOfWeek.Thursday => WorkingDayFlags.Thursday,
        DayOfWeek.Friday => WorkingDayFlags.Friday,
        DayOfWeek.Saturday => WorkingDayFlags.Saturday,
        DayOfWeek.Sunday => WorkingDayFlags.Sunday,
        _ => WorkingDayFlags.None,
    });

    public bool IsDateWithinGracePeriod(TimeOnly actualTime)
    {
        var lateThreshold = StartTime.AddMinutes(GracePeriodMinutes);
        return actualTime >= StartTime && actualTime <= lateThreshold;
    }

    public bool IsDateLate(TimeOnly actualTime)
    {
        var lateThreshold = StartTime.AddMinutes(GracePeriodMinutes + LateThresholdMinutes);
        return actualTime > StartTime.AddMinutes(GracePeriodMinutes) && actualTime <= lateThreshold;
    }

    public bool IsEarlyDeparture(TimeOnly actualTime)
    {
        var earlyThreshold = EndTime.AddMinutes(-EarlyDepartureThresholdMinutes);
        return actualTime >= earlyThreshold && actualTime < EndTime;
    }

    /// <summary>
    /// Checks if worked minutes are below the minimum threshold for a valid workday.
    /// </summary>
    public bool IsBelowMinimumWorkTime(int workedMinutes) => workedMinutes < MinimumWorkMinutesForPresence;
}
