namespace HRPayroll.Application.DTOs.Attendance;

public record AttendanceViewerItemDto
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FirstPunchIn { get; init; }
    public string? LastPunchOut { get; init; }
    public int NetWorkedMinutes { get; init; }
    public int LateMinutes { get; init; }
    public int EarlyDepartureMinutes { get; init; }
    public int OvertimeMinutes { get; init; }
    public bool IsOnLeave { get; init; }
    public bool IsHoliday { get; init; }
    public string? Notes { get; init; }
}

public record AttendanceViewerSummaryDto
{
    public int TotalPresentDays { get; init; }
    public int TotalLateOccurrences { get; init; }
    public int TotalAbsentDays { get; init; }
    public int TotalLeaveDays { get; init; }
    public int TotalHolidayDays { get; init; }
    public decimal TotalOvertimeHours { get; init; }
    public int TotalWorkedMinutes { get; init; }
}

public record AttendanceViewerResult
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }
    public List<AttendanceViewerItemDto> Days { get; init; } = new();
    public AttendanceViewerSummaryDto Summary { get; init; } = new();
}
