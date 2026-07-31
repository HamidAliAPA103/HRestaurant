using HRestaurant.Enum;

namespace HRestaurant.DTOS.Inventory;

public sealed class InventoryItemCreateDTO
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid IngredientId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public IngredientUnit Unit { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
}
