using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.MenuCategory;

public sealed class MenuCategoryListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public bool? IsActive { get; set; }
}
