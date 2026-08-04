namespace AssignmentHub.Application.Common.Models;

/// <summary>Common query-string convention for every list endpoint: ?page=&amp;pageSize=&amp;sortBy=&amp;sortDir=&amp;search=</summary>
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string SortDir { get; set; } = "asc";

    public bool IsDescending => string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);
}
