namespace HRestaurant.DTOS.OrderItem;

public sealed class OrderItemCreatDTO
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? KitchenNote { get; set; }
}

public sealed class OrderItemAddDTO
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? KitchenNote { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
