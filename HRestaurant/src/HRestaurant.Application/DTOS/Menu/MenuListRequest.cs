using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.Menu;

public sealed class MenuListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsPopular { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
}
