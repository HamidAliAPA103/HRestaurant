using HRestaurant.Enum;

namespace HRestaurant.DTOS.Order;

public sealed class OrderStatusUpdateDTO
{
    public OrderStatus Status { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class OrderCancelDTO
{
    public string Reason { get; set; } = string.Empty;
    public bool RequestRefund { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class OrderTableUpdateDTO
{
    public Guid TableId { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class OrderDiscountDTO
{
    public decimal DiscountPercentage { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class OrderMergeDTO
{
    public List<Guid> SourceOrderIds { get; set; } = [];
    public byte[] RowVersion { get; set; } = [];
}

public sealed class OrderSplitItemDTO
{
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
}

public sealed class OrderSplitDTO
{
    public List<OrderSplitItemDTO> Items { get; set; } = [];
    public Guid? TableId { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class OrderConcurrencyDTO
{
    public byte[] RowVersion { get; set; } = [];
}
