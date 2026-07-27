namespace HRestaurant.DTOS.Responses;

public sealed class PaginationRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int PageNumber { get; set; } = DefaultPageNumber;

    public int PageSize { get; set; } = DefaultPageSize;
}
