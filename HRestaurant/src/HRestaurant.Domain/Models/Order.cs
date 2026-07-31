using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class Order : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? TableId { get; set; }
    public Guid? WaiterId { get; set; }
    public Guid? CustomerId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal OrderDiscountPercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? PreparingAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? InventoryConsumedAt { get; set; }
    public DateTime? InventoryReturnedAt { get; set; }
    public bool IsPriority { get; set; }
    public bool IsPaid { get; set; }
    public decimal PaidAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public bool RefundRequired { get; set; }
    public DateTime? RefundedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Restaurant Restaurant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public User? Customer { get; set; }
    public User? Waiter { get; set; }
    public Table? Table { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
    public List<LoyaltyTransaction> LoyaltyTransactions { get; set; } = [];
}
