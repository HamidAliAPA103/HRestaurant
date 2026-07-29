using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces;

public interface IOrderService :
    ICrudService<OrderCreatDTO, OrderUpdateDTO, OrderGetDTO>
{
    Task<ApiResponse<object?>> UpdateKitchenStatusAsync(
        Guid id,
        OrderStatus status,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> ProcessPaymentAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
