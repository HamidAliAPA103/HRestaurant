using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? KitchenNote { get; set; }
    public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;
    public Order Order { get; set; } = null!;
    public Menu MenuItem { get; set; } = null!;
}
