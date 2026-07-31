namespace HRestaurant.DTOS.Public;

public sealed class PublicRestaurantTableDto
{
    public Guid Id { get; set; }

    public string TableNumber { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Shape { get; set; } = string.Empty;

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public double PositionZ { get; set; }

    public double RotationX { get; set; }

    public double RotationY { get; set; }

    public double RotationZ { get; set; }

    public double Width { get; set; }

    public double Length { get; set; }

    public double Height { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public string? UnavailableReason { get; set; }
}
