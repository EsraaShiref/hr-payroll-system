namespace HRPayroll.Application.DTOs.Attendance;

public record AttendanceExceptionDto
{
    public Guid Id { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public string ExceptionType { get; init; } = string.Empty; // UnresolvedShift, UnpairedPunch, UnexcusedAbsence, LeavePunchConflict, StaleUnprocessedPunches
    public string Severity { get; init; } = string.Empty; // Warning, Error
    public string Details { get; init; } = string.Empty;
    public Guid? SummaryId { get; init; } // null if no summary exists yet
    public bool CanOverride { get; init; }
}
