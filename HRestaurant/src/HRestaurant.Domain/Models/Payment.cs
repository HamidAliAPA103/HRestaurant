using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string? TransactionReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Order Order { get; set; } = null!;
    public Restaurant Restaurant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public List<Refund> Refunds { get; set; } = [];
}
