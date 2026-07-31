using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.Inventory;

public sealed class InventoryListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? IngredientId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
