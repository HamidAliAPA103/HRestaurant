using HRestaurant.Enum;

namespace HRestaurant.DTOS.Inventory;

public sealed class StockMovementDTO
{
    public decimal Quantity { get; set; }
    public StockTransactionType TransactionType { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
