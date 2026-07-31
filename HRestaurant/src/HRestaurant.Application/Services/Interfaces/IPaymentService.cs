using HRestaurant.DTOS.Payment;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<Guid>> CreateAsync(PaymentCreateDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderPaymentSummaryDTO>> CompleteAsync(Guid id, PaymentCompleteDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderPaymentSummaryDTO>> FailAsync(Guid id, PaymentFailedDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderPaymentSummaryDTO>> SplitAsync(SplitPaymentDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderPaymentSummaryDTO>> GetOrderSummaryAsync(Guid orderId,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<ReceiptDTO>> GetReceiptAsync(Guid orderId,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderPaymentSummaryDTO>> RefundAsync(Guid paymentId, RefundCreateDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<RefundGetDTO>>> GetRefundHistoryAsync(Guid paymentId,
        CancellationToken cancellationToken = default);
}
