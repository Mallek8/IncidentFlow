namespace IncidentFlow.Application.Common.Models;

public class PaginationQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public string? Search { get; set; }
}
