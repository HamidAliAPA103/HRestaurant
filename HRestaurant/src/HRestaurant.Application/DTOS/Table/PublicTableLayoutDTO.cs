namespace HRestaurant.DTOS.Table;

public sealed class PublicTableLayoutDTO
{
    public Guid Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Shape { get; set; } = string.Empty;
    public TableVectorDTO Position { get; set; } = new();
    public TableVectorDTO Rotation { get; set; } = new();
    public TableDimensionsDTO Dimensions { get; set; } = new();
    public string PublicStatus { get; set; } = string.Empty;
}

public sealed class TableVectorDTO
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}

public sealed class TableDimensionsDTO
{
    public double Width { get; set; }
    public double Length { get; set; }
    public double Height { get; set; }
}
