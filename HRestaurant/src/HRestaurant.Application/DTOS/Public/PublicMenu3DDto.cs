namespace HRestaurant.DTOS.Public;

public sealed class PublicMenuItem3DDto
{
    public Guid Id { get; init; }
    public string RestaurantSlug { get; init; } = string.Empty;
    public string RestaurantName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
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
    public string? Model3DUrl { get; init; }
    public string? ModelPosterUrl { get; init; }
    public decimal ModelScale { get; init; }
    public decimal ModelRotationX { get; init; }
    public decimal ModelRotationY { get; init; }
    public decimal ModelRotationZ { get; init; }
    public bool Is3DEnabled { get; init; }
    public bool EnableIngredientAnimation { get; init; }
    public bool UsesProceduralFallback =>
        !Is3DEnabled || string.IsNullOrWhiteSpace(Model3DUrl);
}

public sealed class PublicIngredient3DDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public decimal RequiredQuantity { get; init; }
    public string? Model3DUrl { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public decimal? Calories { get; init; }
    public decimal? Protein { get; init; }
    public decimal? Carbohydrates { get; init; }
    public decimal? Fat { get; init; }
    public string? Origin { get; init; }
    public string? AllergenInformation { get; init; }
    public decimal ExplodedPositionX { get; init; }
    public decimal ExplodedPositionY { get; init; }
    public decimal ExplodedPositionZ { get; init; }
    public decimal ExplodedRotationX { get; init; }
    public decimal ExplodedRotationY { get; init; }
    public decimal ExplodedRotationZ { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsVisibleIn3D { get; init; }
    public string FallbackKind { get; init; } = "generic";
    public bool UsesProceduralFallback => string.IsNullOrWhiteSpace(Model3DUrl);
}
