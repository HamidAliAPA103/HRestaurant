using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class LoyaltyTransaction : BaseEntity
{
    public Guid LoyaltyAccountId { get; set; }
    public Guid? OrderId { get; set; }
    public LoyaltyTransactionType Type { get; set; }
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public LoyaltyAccount LoyaltyAccount { get; set; } = null!;
    public Order? Order { get; set; }
}
