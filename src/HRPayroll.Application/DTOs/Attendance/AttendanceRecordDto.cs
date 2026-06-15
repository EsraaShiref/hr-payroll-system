namespace HRPayroll.Application.DTOs.Attendance;

public record AttendanceRecordDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public TimeOnly? ClockIn { get; init; }
    public TimeOnly? ClockOut { get; init; }
    public int BreakDurationMinutes { get; init; }
    public int WorkedMinutes { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
