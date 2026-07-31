namespace HRestaurant.DTOS.Table;

public sealed class TableLayoutItemDTO
{
    public Guid TableId { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double PositionZ { get; set; }
    public double RotationX { get; set; }
    public double RotationY { get; set; }
    public double RotationZ { get; set; }
    public double Width { get; set; }
    public double Length { get; set; }
}

public sealed class TableLayoutBulkUpdateDTO
{
    public Guid BranchId { get; set; }
    public List<TableLayoutItemDTO> Tables { get; set; } = [];
}
