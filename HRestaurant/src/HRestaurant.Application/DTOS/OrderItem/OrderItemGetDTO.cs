using HRestaurant.Enum;

namespace HRestaurant.DTOS.OrderItem;

public sealed class OrderItemGetDTO
{
    public Guid ID { get; set; }
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? KitchenNote { get; set; }
    public OrderItemStatus Status { get; set; }
}
