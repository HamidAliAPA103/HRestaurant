namespace HRestaurant.DTOS.OrderItem;

public sealed class OrderItemUpdateDTO
{
    public int Quantity { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class OrderItemKitchenNoteDTO
{
    public string? KitchenNote { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
