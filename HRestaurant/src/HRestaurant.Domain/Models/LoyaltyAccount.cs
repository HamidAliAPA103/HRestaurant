using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class LoyaltyAccount : BaseEntity
{
    public Guid CustomerId { get; set; }
    public int CurrentPoints { get; set; }
    public int LifetimeEarnedPoints { get; set; }
    public int LifetimeRedeemedPoints { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public User Customer { get; set; } = null!;
    public List<LoyaltyTransaction> Transactions { get; set; } = [];
}
