using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class InventoryNotification : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? InventoryItemId { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public InventoryAlertType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
