using HRestaurant.DTOS.OrderItem;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Order;

public sealed class OrderCreatDTO
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? TableId { get; set; }
    public Guid? WaiterId { get; set; }
    public Guid? CustomerId { get; set; }
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public string? Notes { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsPriority { get; set; }
    public List<OrderItemCreatDTO> Items { get; set; } = [];
}
