using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.UploadAttendanceFile;

public sealed record UploadAttendanceFileCommand(
    string FileName,
    byte[] FileContent) : IRequest<ErrorOr<DTOs.Attendance.UploadAttendanceFileResultDto>>;
