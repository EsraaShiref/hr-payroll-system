using HRPayroll.Application.Interfaces;

namespace HRPayroll.Infrastructure.Services.FileParsing;

public class FileParserService : IFileParserService
{
    private readonly CsvParserService _csv;
    private readonly ExcelParserService _excel;
    private static readonly HashSet<string> CsvExtensions = new(StringComparer.OrdinalIgnoreCase) { ".csv" };
    private static readonly HashSet<string> ExcelExtensions = new(StringComparer.OrdinalIgnoreCase) { ".xlsx", ".xls" };

    public FileParserService(CsvParserService csv, ExcelParserService excel)
    {
        _csv = csv;
        _excel = excel;
    }

    public Task<FileParseResult> ParseAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName);

        if (CsvExtensions.Contains(ext))
            return _csv.ParseAsync(fileName, content, ct);

        if (ExcelExtensions.Contains(ext))
            return _excel.ParseAsync(fileName, content, ct);

        throw new InvalidOperationException($"Unsupported file format: {ext}. Supported formats: CSV, XLSX, XLS.");
    }
}
