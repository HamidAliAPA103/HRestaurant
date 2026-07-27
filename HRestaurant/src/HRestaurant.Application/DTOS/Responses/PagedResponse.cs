namespace HRestaurant.DTOS.Responses;

public sealed class PagedResponse<T> : ApiResponse<IReadOnlyCollection<T>>
{
    private PagedResponse(
        IReadOnlyCollection<T> data,
        int pageNumber,
        int pageSize,
        int totalCount,
        string message)
        : base(
            true,
            message,
            data,
            Array.Empty<ErrorResponse>(),
            200)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResponse<T> Create(
        IReadOnlyCollection<T> data,
        int pageNumber,
        int pageSize,
        int totalCount,
        string message = "Resources retrieved successfully.")
    {
        return new PagedResponse<T>(
            data,
            pageNumber,
            pageSize,
            totalCount,
            message);
    }
}
