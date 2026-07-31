using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class Refund : BaseEntity
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid RefundedByUserId { get; set; }
    public DateTime RefundedAt { get; set; }
    public Payment Payment { get; set; } = null!;
}
