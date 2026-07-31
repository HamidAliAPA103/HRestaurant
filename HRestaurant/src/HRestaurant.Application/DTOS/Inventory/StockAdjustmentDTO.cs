namespace HRestaurant.DTOS.Inventory;

public sealed class StockAdjustmentDTO
{
    public decimal NewQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
