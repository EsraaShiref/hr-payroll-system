using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

/// <summary>
/// Parses attendance punch files (Excel or CSV) into raw punch records.
/// </summary>
public interface IFileParserService
{
    /// <returns>Parsed punches mapped to employee IDs, plus per-row errors.</returns>
    Task<FileParseResult> ParseAsync(string fileName, Stream content, CancellationToken ct = default);
}

public record FileParseResult
{
    public int TotalRows { get; init; }
    public List<ParsedPunch> SuccessfulPunches { get; init; } = new();
    public List<ParseRowError> Errors { get; init; } = new();
}

public record ParsedPunch(
    string EmployeeCode,
    DateTime TimestampUtc,
    Domain.Enums.PunchType Type,
    string? DeviceId);

public record ParseRowError(int RowNumber, string ErrorMessage);
