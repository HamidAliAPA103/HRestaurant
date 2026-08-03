using HRestaurant.Enum;

namespace HRestaurant.DTOS.Ingredient;

public sealed class IngredientUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; }
    public bool IsActive { get; set; }
    public string? Model3DUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public decimal? Calories { get; set; }
    public decimal? Protein { get; set; }
    public decimal? Carbohydrates { get; set; }
    public decimal? Fat { get; set; }
    public string? Origin { get; set; }
    public string? AllergenInformation { get; set; }
}
