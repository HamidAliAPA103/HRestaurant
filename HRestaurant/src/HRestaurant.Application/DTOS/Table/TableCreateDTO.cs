using HRestaurant.Enum;

namespace HRestaurant.DTOS.Table;

public sealed class TableCreateDTO
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public TableStatus Status { get; set; } = TableStatus.Available;
    public TableShape Shape { get; set; } = TableShape.Round;
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double PositionZ { get; set; }
    public double RotationX { get; set; }
    public double RotationY { get; set; }
    public double RotationZ { get; set; }
    public double Width { get; set; } = 1.8;
    public double Length { get; set; } = 1.8;
    public double Height { get; set; } = 0.75;
    public bool IsActive { get; set; } = true;
}
