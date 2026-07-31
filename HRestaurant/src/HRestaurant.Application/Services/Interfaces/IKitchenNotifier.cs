using HRestaurant.DTOS.Order;

namespace HRestaurant.Services.Interfaces;

public interface IKitchenNotifier
{
    Task OrderCreatedAsync(KitchenOrderEventDTO notification,
        CancellationToken cancellationToken = default);
    Task OrderUpdatedAsync(KitchenOrderEventDTO notification,
        CancellationToken cancellationToken = default);
    Task OrderStatusChangedAsync(KitchenOrderEventDTO notification,
        CancellationToken cancellationToken = default);
    Task OrderCancelledAsync(KitchenOrderEventDTO notification,
        CancellationToken cancellationToken = default);
    Task OrderReadyAsync(KitchenOrderEventDTO notification, Guid? waiterAppUserId,
        CancellationToken cancellationToken = default);
}
