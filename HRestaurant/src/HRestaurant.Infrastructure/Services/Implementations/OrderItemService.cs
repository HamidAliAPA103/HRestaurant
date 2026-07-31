using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class OrderItemService : IOrderItemService
{
    private readonly IOrderService _orders;

    public OrderItemService(IOrderService orders) => _orders = orders;

    public Task<ApiResponse<object?>> AddAsync(
        Guid orderId, OrderItemAddDTO dto, CancellationToken cancellationToken = default) =>
        _orders.AddItemAsync(orderId, dto, cancellationToken);

    public Task<ApiResponse<object?>> UpdateQuantityAsync(
        Guid orderId, Guid itemId, OrderItemUpdateDTO dto,
        CancellationToken cancellationToken = default) =>
        _orders.UpdateItemQuantityAsync(orderId, itemId, dto, cancellationToken);

    public Task<ApiResponse<object?>> UpdateKitchenNoteAsync(
        Guid orderId, Guid itemId, OrderItemKitchenNoteDTO dto,
        CancellationToken cancellationToken = default) =>
        _orders.UpdateItemKitchenNoteAsync(orderId, itemId, dto, cancellationToken);

    public Task<ApiResponse<object?>> RemoveAsync(
        Guid orderId, Guid itemId, byte[] rowVersion,
        CancellationToken cancellationToken = default) =>
        _orders.RemoveItemAsync(orderId, itemId, rowVersion, cancellationToken);
}
