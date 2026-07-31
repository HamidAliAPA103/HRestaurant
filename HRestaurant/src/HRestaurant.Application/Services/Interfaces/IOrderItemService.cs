using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IOrderItemService
{
    Task<ApiResponse<object?>> AddAsync(Guid orderId, OrderItemAddDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateQuantityAsync(Guid orderId, Guid itemId,
        OrderItemUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateKitchenNoteAsync(Guid orderId, Guid itemId,
        OrderItemKitchenNoteDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> RemoveAsync(Guid orderId, Guid itemId,
        byte[] rowVersion, CancellationToken cancellationToken = default);
}
