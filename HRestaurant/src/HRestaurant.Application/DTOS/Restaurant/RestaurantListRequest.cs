using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Restaurant;

public sealed class RestaurantListRequest
{
    public int PageNumber { get; set; } =
        PaginationRequest.DefaultPageNumber;

    public int PageSize { get; set; } =
        PaginationRequest.DefaultPageSize;

    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    public ViewType Type { get; set; } = ViewType.notdeleted;

    public string SortBy { get; set; } = "createdAt";

    public string SortDirection { get; set; } = "desc";
}
