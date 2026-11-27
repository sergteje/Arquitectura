namespace BuildingBlocks.Abstractions.Pagination;

/// <summary>
/// Page selector for paginated queries.
/// </summary>
public record PaginationRequest
{
    /// <summary>
    /// Gets the one-based page number.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Gets how many rows to skip.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
}

/// <summary>
/// One page of results plus its paging metadata.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public record PaginatedResponse<T>
{
    /// <summary>
    /// Gets the items on this page.
    /// </summary>
    public required IList<T> Items { get; init; }

    /// <summary>
    /// Gets the one-based page number.
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of rows across all pages.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Creates a paginated response.
    /// </summary>
    /// <param name="items">The items on this page.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total number of rows.</param>
    /// <returns>The paginated response.</returns>
    public static PaginatedResponse<T> Create(IList<T> items, int page, int pageSize, int totalCount) => new()
    {
        Items = items,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };
}

/// <summary>
/// Read-only page of results, used by in-memory specifications.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PaginatedResult<T>
{
    /// <summary>
    /// Gets the items on this page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the total number of rows across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the one-based page number.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
