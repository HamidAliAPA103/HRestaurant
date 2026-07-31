using HRestaurant.Enum;

namespace HRestaurant.DTOS.Table;

public sealed class TableUpdateDTO
{
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
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
}
