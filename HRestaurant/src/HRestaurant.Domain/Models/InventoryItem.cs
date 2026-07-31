using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class InventoryItem : BaseEntity
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
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = [];
    public Restaurant Restaurant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public List<StockTransaction> Transactions { get; set; } = [];
    public List<InventoryNotification> Notifications { get; set; } = [];
}
