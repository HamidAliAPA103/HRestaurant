using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Loyalty;

public sealed class LoyaltyAdjustmentDTO
{
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
}

public sealed class LoyaltyHistoryRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
}

public sealed class LoyaltyTransactionGetDTO
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public LoyaltyTransactionType Type { get; set; }
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class LoyaltySummaryDTO
{
    public Guid CustomerId { get; set; }
    public int CurrentPoints { get; set; }
    public int LifetimeEarnedPoints { get; set; }
    public int LifetimeRedeemedPoints { get; set; }
    public decimal CurrencyValue { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
