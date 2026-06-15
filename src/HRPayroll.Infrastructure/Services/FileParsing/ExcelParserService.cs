using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Enums;
using ClosedXML.Excel;

namespace HRPayroll.Infrastructure.Services.FileParsing;

public class ExcelParserService : IFileParserService
{
    public Task<FileParseResult> ParseAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var result = new FileParseResult();
        var errors = new List<ParseRowError>();
        var punches = new List<ParsedPunch>();

        using var workbook = new XLWorkbook(content);
        var ws = workbook.Worksheets.FirstOrDefault();
        if (ws == null)
        {
            errors.Add(new ParseRowError(0, "No worksheets found in the Excel file"));
            return Task.FromResult(result with { Errors = errors });
        }

        var firstRow = ws.Row(1);
        var totalColumns = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
        var headers = Enumerable.Range(1, totalColumns)
            .Select(col => firstCellValue(ws, 1, col))
            .ToArray();

        var empCodeIdx = IndexOfHeader(headers, "EmployeeCode", "Employee Code", "EmpCode");
        var timestampIdx = IndexOfHeader(headers, "TimestampUtc", "Timestamp", "DateTime", "Date/Time");
        var typeIdx = IndexOfHeader(headers, "Type", "PunchType", "Punch Type");
        var deviceIdx = IndexOfHeader(headers, "DeviceId", "Device ID", "TerminalId", "Terminal ID");

        if (empCodeIdx < 0 || timestampIdx < 0 || typeIdx < 0)
        {
            errors.Add(new ParseRowError(0,
                $"Missing required columns. Found: {string.Join(", ", headers)}. " +
                "Required: EmployeeCode, TimestampUtc, Type"));
            return Task.FromResult(result with { Errors = errors });
        }

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int row = 2; row <= lastRow; row++)
        {
            ct.ThrowIfCancellationRequested();

            var employeeCode = cellValue(ws, row, empCodeIdx + 1);
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                errors.Add(new ParseRowError(row, "EmployeeCode is empty"));
                continue;
            }

            var timestampStr = cellValue(ws, row, timestampIdx + 1);
            if (string.IsNullOrWhiteSpace(timestampStr))
            {
                errors.Add(new ParseRowError(row, "Timestamp is empty"));
                continue;
            }

            if (!DateTime.TryParse(timestampStr, out var timestamp))
            {
                errors.Add(new ParseRowError(row, $"Invalid timestamp: {timestampStr}"));
                continue;
            }

            var typeStr = cellValue(ws, row, typeIdx + 1);
            if (!Enum.TryParse<PunchType>(typeStr, ignoreCase: true, out var punchType))
            {
                errors.Add(new ParseRowError(row, $"Invalid punch type: {typeStr}. Expected In or Out."));
                continue;
            }

            var deviceId = deviceIdx >= 0 ? cellValue(ws, row, deviceIdx + 1) : null;

            punches.Add(new ParsedPunch(employeeCode, timestamp.ToUniversalTime(), punchType, string.IsNullOrWhiteSpace(deviceId) ? null : deviceId));
        }

        return Task.FromResult(new FileParseResult
        {
            TotalRows = lastRow,
            SuccessfulPunches = punches,
            Errors = errors
        });
    }

    private static string cellValue(IXLWorksheet ws, int row, int col)
    {
        var cell = ws.Cell(row, col);
        return cell.Value.ToString()?.Trim() ?? string.Empty;
    }

    private static string firstCellValue(IXLWorksheet ws, int row, int col)
    {
        var cell = ws.Cell(row, col);
        if (!cell.Value.IsText && cell.Value.IsNumber)
            return string.Empty;
        return cell.Value.ToString()?.Trim() ?? string.Empty;
    }

    private static int IndexOfHeader(string[] headers, params string[] candidates)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (candidates.Any(c => string.Equals(headers[i], c, StringComparison.OrdinalIgnoreCase)))
                return i;
        }
        return -1;
    }
}
