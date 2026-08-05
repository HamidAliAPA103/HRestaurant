namespace HRestaurant.DTOS.Public;

public sealed class PublicMenuCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyCollection<PublicMenuItemDto> Items { get; init; } = [];
}

public sealed class PublicMenuItemDto
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Nutrition { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal Price { get; init; }
    public decimal DiscountPercentage { get; init; }
    public decimal FinalPrice { get; init; }
    public int PreparationTimeMinutes { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsPopular { get; init; }
    public bool Is3DEnabled { get; init; }
    public bool Has3DModel { get; init; }
    public bool EnableIngredientAnimation { get; init; }
    public string? ModelPosterUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? VideoPosterUrl { get; init; }
    public int? VideoDurationSeconds { get; init; }
    public bool IsVideoEnabled { get; init; }
    public int VideoDisplayOrder { get; init; }
    public IReadOnlyCollection<string> Ingredients { get; init; } = [];
}
