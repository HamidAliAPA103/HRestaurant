using HRestaurant.DTOS.Loyalty;
using HRestaurant.DTOS.Responses;
using HRestaurant.Models;

namespace HRestaurant.Services.Interfaces;

public interface ILoyaltyService
{
    Task<ApiResponse<LoyaltySummaryDTO>> GetSummaryAsync(Guid customerId,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<LoyaltyTransactionGetDTO>> GetHistoryAsync(Guid customerId,
        LoyaltyHistoryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoyaltySummaryDTO>> AdjustAsync(Guid customerId, LoyaltyAdjustmentDTO dto,
        CancellationToken cancellationToken = default);
    Task RedeemForPaymentAsync(Order order, Payment payment,
        CancellationToken cancellationToken = default);
    Task EarnForFullyPaidOrderAsync(Order order,
        CancellationToken cancellationToken = default);
}
