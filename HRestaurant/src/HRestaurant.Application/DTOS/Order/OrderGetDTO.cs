using HRestaurant.DTOS.OrderItem;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Order;

public sealed class OrderGetDTO
{
    public Guid ID { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid? TableId { get; set; }
    public string? TableNumber { get; set; }
    public Guid? WaiterId { get; set; }
    public string? WaiterName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal OrderDiscountPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPrices => TotalAmount;
    public string? Notes { get; set; }
    public bool IsPriority { get; set; }
    public bool IsPaid { get; set; }
    public bool RefundRequired { get; set; }
    public DateTime CreatAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public DateTime? PreparingAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<OrderItemGetDTO> Items { get; set; } = [];
}
