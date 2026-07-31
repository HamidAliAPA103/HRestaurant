using HRestaurant.Enum;
using HRestaurant.DTOS.OrderItem;

namespace HRestaurant.DTOS.Order;

public sealed class KitchenDashboardDTO
{
    public int PendingCount { get; set; }
    public int PreparingCount { get; set; }
    public int ReadyCount { get; set; }
    public double AveragePreparationMinutes { get; set; }
    public IReadOnlyCollection<KitchenOrderDTO> Orders { get; set; } = [];
}

public sealed class KitchenOrderDTO
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string? TableNumber { get; set; }
    public string? WaiterName { get; set; }
    public IReadOnlyCollection<string> KitchenNotes { get; set; } = [];
    public IReadOnlyCollection<OrderItemGetDTO> Items { get; set; } = [];
    public double PreparationDurationMinutes { get; set; }
    public bool IsDelayed { get; set; }
    public bool IsPriority { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed record KitchenOrderEventDTO(
    string EventName,
    KitchenOrderDTO Order,
    DateTime OccurredAtUtc,
    string? AudioCue = null);
