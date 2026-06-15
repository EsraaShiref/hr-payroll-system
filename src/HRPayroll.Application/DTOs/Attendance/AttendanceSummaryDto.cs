namespace HRPayroll.Application.DTOs.Attendance;

public record AttendanceSummaryDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }
    public int PresentDays { get; init; }
    public int AbsentDays { get; init; }
    public int LateDays { get; init; }
    public int HalfDays { get; init; }
    public int LeaveDays { get; init; }
    public decimal TotalWorkedMinutes { get; init; }
}
