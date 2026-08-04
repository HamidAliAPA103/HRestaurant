namespace HRestaurant.DTOS.Menu;

public sealed class MenuUpdateDTO
{
    public HRestaurant.DTOS.Common.FileUploadDTO? Image { get; set; }
    public string? ImageURL { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Desc { get; set; }
    public string? Nutrition { get; set; }
    public string? Model3DUrl { get; set; }
    public string? ModelPosterUrl { get; set; }
    public decimal? ModelScale { get; set; }
    public decimal? ModelRotationX { get; set; }
    public decimal? ModelRotationY { get; set; }
    public decimal? ModelRotationZ { get; set; }
    public bool? Is3DEnabled { get; set; }
    public string? VideoUrl { get; set; }
    public string? VideoPosterUrl { get; set; }
    public int? VideoDurationSeconds { get; set; }
    public bool? IsVideoEnabled { get; set; }
    public int? VideoDisplayOrder { get; set; }
    public List<MenuItemIngredientDTO>? Ingredients { get; set; }
}
