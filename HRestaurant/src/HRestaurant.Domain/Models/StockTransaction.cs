using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class StockTransaction : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public StockTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public Guid CreatedByUserId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
}
