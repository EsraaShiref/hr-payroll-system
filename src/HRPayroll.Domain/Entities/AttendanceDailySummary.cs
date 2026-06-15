using HRPayroll.Domain.Enums;

namespace HRPayroll.Domain.Entities;

public class AttendanceDailySummary : BaseEntity
{
    // Identity
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateOnly Date { get; private set; }

    // Shift snapshot (audit trail — mirrors TaxBracketSetId pattern from Module B)
    public Guid ShiftId { get; private set; }
    public string ShiftName { get; private set; } = string.Empty;
    public TimeOnly ScheduledStart { get; private set; }
    public TimeOnly ScheduledEnd { get; private set; }
    public int GracePeriodMinutes { get; private set; }
    public int LateThresholdMinutes { get; private set; }
    public int EarlyDepartureThresholdMinutes { get; private set; }
    public int OvertimeThresholdMinutes { get; private set; }
    public int MinimumWorkMinutesForPresence { get; private set; }

    // Raw punch data
    public TimeOnly? FirstPunchIn { get; private set; }
    public TimeOnly? LastPunchOut { get; private set; }
    public int TotalBreakMinutes { get; private set; }

    // Calculated outputs
    public int TotalWorkedMinutes { get; private set; }
    public int LateMinutes { get; private set; }
    public int EarlyDepartureMinutes { get; private set; }
    public int OvertimeMinutes { get; private set; }
    public bool IsUnexcusedAbsence { get; private set; }
    public bool IsOnLeave { get; private set; }
    public Guid? LeaveRequestId { get; private set; }
    public bool IsHoliday { get; private set; }
    public Guid? HolidayId { get; private set; }

    // HR manual overrides before payroll lock
    public TimeOnly? OverridePunchIn { get; private set; }
    public TimeOnly? OverridePunchOut { get; private set; }
    public string? OverrideReason { get; private set; }

    // Notes
    public string? Notes { get; private set; }

    // Audit
    public DateTime CalculatedAt { get; private set; }
    public string CalculatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Display-friendly classification. DERIVED from boolean flags and minute fields —
    /// never stored independently. Query by the machine-readable flags (IsUnexcusedAbsence,
    /// IsOnLeave, IsHoliday, LateMinutes > 0, EarlyDepartureMinutes > 0) instead.
    /// Invariant: Holiday > OnLeave > PendingReview > AbsentUnexcused > Late > EarlyDeparture > OnTime
    /// </summary>
    public AttendanceSummaryStatus Status
    {
        get
        {
            if (IsHoliday) return AttendanceSummaryStatus.Holiday;
            if (IsOnLeave) return AttendanceSummaryStatus.OnLeave;

            // Punched in but no punch-out = needs HR review, NOT absence
            var effectiveOut = OverridePunchOut ?? LastPunchOut;
            if (FirstPunchIn.HasValue && !effectiveOut.HasValue)
                return AttendanceSummaryStatus.PendingReview;

            if (IsUnexcusedAbsence) return AttendanceSummaryStatus.AbsentUnexcused;
            if (LateMinutes > 0) return AttendanceSummaryStatus.Late;
            if (EarlyDepartureMinutes > 0) return AttendanceSummaryStatus.EarlyDeparture;
            return AttendanceSummaryStatus.OnTime;
        }
    }

    /// <summary>
    /// Net worked minutes: (last punch out - first punch in) - total break minutes.
    /// </summary>
    public int NetWorkedMinutes => (LastPunchOut.HasValue && FirstPunchIn.HasValue)
        ? Math.Max(0, (ToMin(LastPunchOut.Value) - ToMin(FirstPunchIn.Value)) - TotalBreakMinutes)
        : 0;

    private AttendanceDailySummary() { }

    public AttendanceDailySummary(
        Guid employeeId,
        DateOnly date,
        Guid shiftId,
        string shiftName,
        TimeOnly scheduledStart,
        TimeOnly scheduledEnd,
        int gracePeriodMinutes,
        int lateThresholdMinutes,
        int earlyDepartureThresholdMinutes,
        int overtimeThresholdMinutes,
        int minimumWorkMinutesForPresence,
        string calculatedBy)
    {
        EmployeeId = employeeId;
        Date = date;

        ShiftId = shiftId;
        ShiftName = shiftName;
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
        GracePeriodMinutes = gracePeriodMinutes;
        LateThresholdMinutes = lateThresholdMinutes;
        EarlyDepartureThresholdMinutes = earlyDepartureThresholdMinutes;
        OvertimeThresholdMinutes = overtimeThresholdMinutes;
        MinimumWorkMinutesForPresence = minimumWorkMinutesForPresence;

        CalculatedBy = calculatedBy;
        CalculatedAt = DateTime.UtcNow;
    }

    public void SetPunchData(TimeOnly? firstPunchIn, TimeOnly? lastPunchOut, int totalBreakMinutes)
    {
        FirstPunchIn = firstPunchIn;
        LastPunchOut = lastPunchOut;
        TotalBreakMinutes = totalBreakMinutes;
    }

    public void Calculate()
    {
        // Reset
        LateMinutes = 0;
        EarlyDepartureMinutes = 0;
        OvertimeMinutes = 0;
        IsUnexcusedAbsence = false;
        TotalWorkedMinutes = 0;

        // Holiday overrides all calculations
        if (IsHoliday) return;

        // On leave: no work expected, no penalties
        if (IsOnLeave) return;

        // No punches at all = unexcused absence
        if (!FirstPunchIn.HasValue)
        {
            IsUnexcusedAbsence = true;
            return;
        }

        // Use overrides if set, else actual punches
        var effectiveIn = OverridePunchIn ?? FirstPunchIn.Value;
        var effectiveOut = OverridePunchOut ?? LastPunchOut;

        // Clocked in but no punch-out: employee was present, NOT an absence.
        // Cannot calculate minutes, requires HR review via ApplyOverride().
        if (!effectiveOut.HasValue)
        {
            // IsUnexcusedAbsence stays false
            // TotalWorkedMinutes stays 0
            // Status getter returns PendingReview
            return;
        }

        TotalWorkedMinutes = (ToMin(effectiveOut.Value) - ToMin(effectiveIn)) - TotalBreakMinutes;
        TotalWorkedMinutes = Math.Max(0, TotalWorkedMinutes);

        // Below minimum work time = unexcused absence
        if (TotalWorkedMinutes < MinimumWorkMinutesForPresence)
        {
            IsUnexcusedAbsence = true;
        }

        // Late check-in
        var graceEnd = ScheduledStart.AddMinutes(GracePeriodMinutes);
        if (effectiveIn > graceEnd)
        {
            LateMinutes = ToMin(effectiveIn) - ToMin(graceEnd);
            LateMinutes = Math.Max(0, LateMinutes);
        }

        // Early departure
        if (effectiveOut < ScheduledEnd)
        {
            EarlyDepartureMinutes = ToMin(ScheduledEnd) - ToMin(effectiveOut.Value);
            EarlyDepartureMinutes = Math.Max(0, EarlyDepartureMinutes);
        }

        // Overtime
        if (effectiveOut > ScheduledEnd)
        {
            OvertimeMinutes = ToMin(effectiveOut.Value) - ToMin(ScheduledEnd);
            OvertimeMinutes = Math.Max(0, OvertimeMinutes);
        }

        CalculatedAt = DateTime.UtcNow;
    }

    public void MarkAsHoliday(Guid holidayId)
    {
        IsHoliday = true;
        HolidayId = holidayId;
    }

    public void MarkAsOnLeave(Guid leaveRequestId)
    {
        IsOnLeave = true;
        LeaveRequestId = leaveRequestId;
    }

    public void ClearLeave()
    {
        IsOnLeave = false;
        LeaveRequestId = null;
    }

    public void ApplyOverride(TimeOnly? punchIn, TimeOnly? punchOut, string reason)
    {
        OverridePunchIn = punchIn;
        OverridePunchOut = punchOut;
        OverrideReason = reason;
        Calculate();
    }

    public void SetNotes(string? notes) => Notes = notes;

    private static int ToMin(TimeOnly t) => t.Hour * 60 + t.Minute;
}
