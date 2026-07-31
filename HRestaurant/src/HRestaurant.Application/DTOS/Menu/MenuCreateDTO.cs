namespace HRestaurant.DTOS.Menu;

public sealed class MenuCreateDTO
{
    public HRestaurant.DTOS.Common.FileUploadDTO? Image { get; set; }
    public string? ImageUrl { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public string Desc { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Nutrition { get; set; } = string.Empty;
    public List<MenuItemIngredientDTO> Ingredients { get; set; } = [];
}
