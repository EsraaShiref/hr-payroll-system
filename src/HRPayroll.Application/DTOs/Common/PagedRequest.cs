namespace HRPayroll.Application.DTOs.Common;

public record PagedRequest
{
    public int PageIndex { get; init; } = 0;
    public int PageSize { get; init; } = 20;
    public string? SortField { get; init; }
    public string? SortDirection { get; init; } = "asc";
    public string? SearchTerm { get; init; }

    public int Skip => PageIndex * PageSize;
}
