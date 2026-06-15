using System.Globalization;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Enums;

namespace HRPayroll.Infrastructure.Services.FileParsing;

public class CsvParserService : IFileParserService
{
    public Task<FileParseResult> ParseAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var result = new FileParseResult();
        var errors = new List<ParseRowError>();
        var punches = new List<ParsedPunch>();

        using var reader = new StreamReader(content);
        string? headerLine = reader.ReadLine();
        if (headerLine == null)
        {
            errors.Add(new ParseRowError(0, "File is empty"));
            return Task.FromResult(result with { Errors = errors });
        }

        var headers = headerLine.Split(',');
        var empCodeIdx = Array.IndexOf(headers, "EmployeeCode");
        var timestampIdx = Array.IndexOf(headers, "TimestampUtc");
        var typeIdx = Array.IndexOf(headers, "Type");
        var deviceIdx = Array.IndexOf(headers, "DeviceId");

        if (empCodeIdx < 0 || timestampIdx < 0 || typeIdx < 0)
        {
            errors.Add(new ParseRowError(0,
                $"Missing required columns. Found: {string.Join(", ", headers)}. " +
                "Required: EmployeeCode, TimestampUtc, Type"));
            return Task.FromResult(result with { Errors = errors });
        }

        int rowNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            rowNumber++;
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = line.Split(',');

            if (fields.Length <= Math.Max(empCodeIdx, Math.Max(timestampIdx, typeIdx)))
            {
                errors.Add(new ParseRowError(rowNumber, $"Row has {fields.Length} columns, expected at least {Math.Max(empCodeIdx, Math.Max(timestampIdx, typeIdx)) + 1}"));
                continue;
            }

            var employeeCode = fields[empCodeIdx].Trim();
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                errors.Add(new ParseRowError(rowNumber, "EmployeeCode is empty"));
                continue;
            }

            if (!DateTime.TryParse(fields[timestampIdx].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp))
            {
                errors.Add(new ParseRowError(rowNumber, $"Invalid timestamp: {fields[timestampIdx]}"));
                continue;
            }

            if (!Enum.TryParse<PunchType>(fields[typeIdx].Trim(), ignoreCase: true, out var punchType))
            {
                errors.Add(new ParseRowError(rowNumber, $"Invalid punch type: {fields[typeIdx]}. Expected In or Out."));
                continue;
            }

            var deviceId = deviceIdx >= 0 && fields.Length > deviceIdx
                ? fields[deviceIdx].Trim()
                : null;

            punches.Add(new ParsedPunch(employeeCode, timestamp.ToUniversalTime(), punchType, deviceId));
        }

        return Task.FromResult(new FileParseResult
        {
            TotalRows = rowNumber,
            SuccessfulPunches = punches,
            Errors = errors
        });
    }
}
