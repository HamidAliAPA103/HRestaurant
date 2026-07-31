using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<Guid>> CreateAsync(OrderCreatDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderGetDTO>> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderGetDTO>> GetAllAsync(OrderListRequest request,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderGetDTO>> GetByBranchAsync(Guid branchId,
        OrderListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderGetDTO>> GetByWaiterAsync(Guid waiterId,
        OrderListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, OrderUpdateDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> AddItemAsync(Guid orderId, OrderItemAddDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateItemQuantityAsync(Guid orderId, Guid itemId,
        OrderItemUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateItemKitchenNoteAsync(Guid orderId, Guid itemId,
        OrderItemKitchenNoteDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> RemoveItemAsync(Guid orderId, Guid itemId,
        byte[] rowVersion, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateStatusAsync(Guid id, OrderStatusUpdateDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateKitchenStatusAsync(Guid id,
        KitchenOrderStatusUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> CancelAsync(Guid id, OrderCancelDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> ChangeTableAsync(Guid id, OrderTableUpdateDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> ApplyDiscountAsync(Guid id, OrderDiscountDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> MergeAsync(Guid targetOrderId, OrderMergeDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<Guid>> SplitAsync(Guid orderId, OrderSplitDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<KitchenDashboardDTO>> GetKitchenDashboardAsync(Guid? branchId,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> ProcessPaymentAsync(Guid id, byte[] rowVersion,
        CancellationToken cancellationToken = default);
}
