using HRestaurant.Enum;

namespace HRestaurant.DTOS.Inventory;

public sealed class StockTransactionGetDTO
{
    public Guid ID { get; set; }
    public Guid InventoryItemId { get; set; }
    public StockTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatAt { get; set; }
}
