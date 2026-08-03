namespace HRestaurant.DTOS.Public;

public sealed class PublicRestaurantExperienceDto
{
    public PublicRestaurantDto Restaurant { get; init; } = new();
    public Guid? DefaultBranchId { get; init; }
}

public sealed class PublicRestaurantSceneDto
{
    public Guid RestaurantId { get; init; }
    public string RestaurantSlug { get; init; } = string.Empty;
    public string RestaurantName { get; init; } = string.Empty;
    public IReadOnlyCollection<PublicBranchSceneDto> Branches { get; init; } = [];
}

public sealed class PublicBranchSceneDto
{
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public double FloorWidth { get; init; }
    public double FloorDepth { get; init; }
    public double WallHeight { get; init; }
    public double CenterX { get; init; }
    public double CenterZ { get; init; }
    public IReadOnlyCollection<PublicSceneTableDto> Tables { get; init; } = [];
    public IReadOnlyCollection<PublicSceneHotspotDto> Hotspots { get; init; } = [];
}

public sealed class PublicSceneTableDto
{
    public Guid Id { get; init; }
    public string TableNumber { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public string Shape { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public double PositionX { get; init; }
    public double PositionY { get; init; }
    public double PositionZ { get; init; }
    public double RotationX { get; init; }
    public double RotationY { get; init; }
    public double RotationZ { get; init; }
    public double Width { get; init; }
    public double Length { get; init; }
    public double Height { get; init; }
}

public sealed class PublicSceneHotspotDto
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double PositionX { get; init; }
    public double PositionY { get; init; }
    public double PositionZ { get; init; }
    public double CameraX { get; init; }
    public double CameraY { get; init; }
    public double CameraZ { get; init; }
    public IReadOnlyCollection<Guid> TableIds { get; init; } = [];
    public int AvailableTableCount { get; init; }
}
