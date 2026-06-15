using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.UploadAttendanceFile;

public class UploadAttendanceFileCommandHandler
    : IRequestHandler<UploadAttendanceFileCommand, ErrorOr<DTOs.Attendance.UploadAttendanceFileResultDto>>
{
    private readonly IFileParserService _fileParser;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendancePunchRepository _punchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadAttendanceFileCommandHandler(
        IFileParserService fileParser,
        IEmployeeRepository employeeRepository,
        IAttendancePunchRepository punchRepository,
        IUnitOfWork unitOfWork)
    {
        _fileParser = fileParser;
        _employeeRepository = employeeRepository;
        _punchRepository = punchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<DTOs.Attendance.UploadAttendanceFileResultDto>> Handle(
        UploadAttendanceFileCommand command, CancellationToken ct)
    {
        using var stream = new MemoryStream(command.FileContent);
        var parseResult = await _fileParser.ParseAsync(command.FileName, stream, ct);

        var result = new DTOs.Attendance.UploadAttendanceFileResultDto
        {
            TotalRows = parseResult.TotalRows,
            FailedRows = parseResult.Errors
                .Select(e => new DTOs.Attendance.ImportRowError(e.RowNumber, e.ErrorMessage))
                .ToList(),
        };

        var duplicateCount = 0;
        var successCount = 0;

        foreach (var parsed in parseResult.SuccessfulPunches)
        {
            var employee = await _employeeRepository.GetByEmployeeCodeAsync(parsed.EmployeeCode, ct);
            if (employee == null)
            {
                result.FailedRows.Add(new DTOs.Attendance.ImportRowError(0,
                    $"Unknown employee code: {parsed.EmployeeCode}"));
                continue;
            }

            var isDuplicate = await _punchRepository.ExistsDuplicateAsync(
                employee.Id, parsed.TimestampUtc, parsed.Type, parsed.DeviceId, ct);

            if (isDuplicate)
            {
                duplicateCount++;
                continue;
            }

            var punch = new AttendancePunch(employee.Id, parsed.TimestampUtc, parsed.Type, PunchSource.Import, parsed.DeviceId);
            _punchRepository.Add(punch);
            successCount++;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return result with
        {
            SuccessfulRows = successCount,
            DuplicatePunchesSkipped = duplicateCount,
        };
    }
}
