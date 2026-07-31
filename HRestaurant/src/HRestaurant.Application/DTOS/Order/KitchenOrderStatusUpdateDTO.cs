using HRestaurant.Enum;

namespace HRestaurant.DTOS.Order;

public sealed class KitchenOrderStatusUpdateDTO
{
    public OrderStatus Status { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
