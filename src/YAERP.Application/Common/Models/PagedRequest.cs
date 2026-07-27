namespace YAERP.Application.Common.Models;

public record PagedRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? SortColumn { get; init; }
    public bool IsDescending { get; init; }
    public string? SearchFilter { get; init; }
}

public record PagedResult<T>(T[] Items, int TotalCount, int PageNumber, int PageSize);
