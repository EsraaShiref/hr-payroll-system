namespace HRPayroll.Application.DTOs.Attendance;

public record ImportRowError(int RowNumber, string ErrorMessage);

public record UploadAttendanceFileResultDto
{
    public int TotalRows { get; init; }
    public int SuccessfulRows { get; init; }
    public int DuplicatePunchesSkipped { get; init; }
    public List<ImportRowError> FailedRows { get; init; } = new();
}
