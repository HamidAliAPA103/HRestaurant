using HRestaurant.DTOS.Order;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HRestaurant.WebApi.Realtime;

public sealed class SignalRKitchenNotifier : IKitchenNotifier
{
    private readonly IHubContext<KitchenHub> _hub;
    public SignalRKitchenNotifier(IHubContext<KitchenHub> hub) => _hub = hub;

    public Task OrderCreatedAsync(KitchenOrderEventDTO notification,
        CancellationToken cancellationToken = default) =>
        BroadcastAsync("OrderCreated", notification, cancellationToken);

    public Task OrderUpdatedAsync(KitchenOrderEventDTO notification,
        CancellationToken cancellationToken = default) =>
        BroadcastAsync("OrderUpdated", notification, cancellationToken);

    public Task OrderStatusChangedAsync(KitchenOrderEventDTO notification,
        CancellationToken cancellationToken = default) =>
        BroadcastAsync("OrderStatusChanged", notification, cancellationToken);

    public Task OrderCancelledAsync(KitchenOrderEventDTO notification,
        CancellationToken cancellationToken = default) =>
        BroadcastAsync("OrderCancelled", notification, cancellationToken);

    public async Task OrderReadyAsync(KitchenOrderEventDTO notification, Guid? waiterAppUserId,
        CancellationToken cancellationToken = default)
    {
        await BroadcastAsync("OrderStatusChanged", notification, cancellationToken);
        var groups = new List<string>
        {
            KitchenHubGroups.Restaurant(notification.Order.RestaurantId),
            KitchenHubGroups.Branch(notification.Order.BranchId),
            KitchenHubGroups.Kitchen(notification.Order.BranchId),
            KitchenHubGroups.Waiters(notification.Order.BranchId)
        };
        if (waiterAppUserId.HasValue)
            groups.Add(KitchenHubGroups.Waiter(waiterAppUserId.Value));
        await _hub.Clients.Groups(groups).SendAsync(
            "OrderReady", notification, cancellationToken);
    }

    private Task BroadcastAsync(string eventName, KitchenOrderEventDTO notification,
        CancellationToken cancellationToken) =>
        _hub.Clients.Groups(
                KitchenHubGroups.Restaurant(notification.Order.RestaurantId),
                KitchenHubGroups.Branch(notification.Order.BranchId),
                KitchenHubGroups.Kitchen(notification.Order.BranchId))
            .SendAsync(eventName, notification, cancellationToken);
}
