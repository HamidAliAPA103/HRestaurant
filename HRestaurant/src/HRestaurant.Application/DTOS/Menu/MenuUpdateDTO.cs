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
    public List<MenuItemIngredientDTO>? Ingredients { get; set; }
}
