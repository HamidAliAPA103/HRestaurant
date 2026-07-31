namespace HRestaurant.DTOS.Order;

public sealed class OrderUpdateDTO
{
    public string? Notes { get; set; }
    public bool IsPriority { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
