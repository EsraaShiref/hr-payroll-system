namespace HRPayroll.Application.DTOs.Attendance;

public record LeaveRequestDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string LeaveType { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal TotalDays { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? RejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
}
