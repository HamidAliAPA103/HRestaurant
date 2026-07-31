using HRestaurant.Enum;

namespace HRestaurant.DTOS.Inventory;

public sealed class InventoryItemUpdateDTO
{
    public Guid? SupplierId { get; set; }
    public decimal MinimumQuantity { get; set; }
    public IngredientUnit Unit { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public bool IsActive { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
