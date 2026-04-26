namespace Arquitectura.Domain.Models.Pagination;

public record PaginationRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int Skip => (Page - 1) * PageSize;
}

public record PaginatedResponse<T>
{
    public required IList<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public static PaginatedResponse<T> Create(IList<T> items, int page, int pageSize, int totalCount) => new()
    {
        Items = items,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };
}