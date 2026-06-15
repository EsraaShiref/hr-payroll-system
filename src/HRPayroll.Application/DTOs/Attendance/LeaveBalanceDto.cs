namespace HRPayroll.Application.DTOs.Attendance;

public record LeaveBalanceDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string LeaveType { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal TotalDays { get; init; }
    public decimal UsedDays { get; init; }
    public decimal RemainingDays { get; init; }
}
