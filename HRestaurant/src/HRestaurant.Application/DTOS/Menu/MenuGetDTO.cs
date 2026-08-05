namespace HRestaurant.DTOS.Menu;

public sealed class MenuGetDTO
{
    public Guid ID { get; set; }
    public DateTime CreatAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string ImageURL { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal FinalPrice { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsPopular { get; set; }
    public string Desc { get; set; } = string.Empty;
    public string Nutrition { get; set; } = string.Empty;
    public string? Model3DUrl { get; set; }
    public string? ModelPosterUrl { get; set; }
    public decimal ModelScale { get; set; }
    public decimal ModelRotationX { get; set; }
    public decimal ModelRotationY { get; set; }
    public decimal ModelRotationZ { get; set; }
    public bool Is3DEnabled { get; set; }
    public bool EnableIngredientAnimation { get; set; }
    public string? VideoUrl { get; set; }
    public string? VideoPosterUrl { get; set; }
    public int? VideoDurationSeconds { get; set; }
    public bool IsVideoEnabled { get; set; }
    public int VideoDisplayOrder { get; set; }
    public List<MenuItemIngredientGetDTO> Ingredients { get; set; } = [];
}
