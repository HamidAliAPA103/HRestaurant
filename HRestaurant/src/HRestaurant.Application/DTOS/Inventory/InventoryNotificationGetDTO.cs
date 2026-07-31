using HRestaurant.Enum;

namespace HRestaurant.DTOS.Inventory;

public sealed class InventoryNotificationGetDTO
{
    public Guid ID { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid InventoryItemId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public InventoryAlertType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime CreatAt { get; set; }
}
