using HRestaurant.Enum;

namespace HRestaurant.DTOS.Inventory;

public sealed class InventoryItemGetDTO
{
    public Guid ID { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public IngredientUnit Unit { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public bool IsActive { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public DateTime CreatAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}
