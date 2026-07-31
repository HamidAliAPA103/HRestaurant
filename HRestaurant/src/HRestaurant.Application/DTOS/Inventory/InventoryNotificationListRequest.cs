using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Inventory;

public sealed class InventoryNotificationListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public InventoryAlertType? Type { get; set; }
    public bool? IsRead { get; set; }
    public bool? IsResolved { get; set; }
}
