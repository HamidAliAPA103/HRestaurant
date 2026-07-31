using HRestaurant.Enum;

namespace HRestaurant.DTOS.Ingredient;

public sealed class IngredientCreateDTO
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; }
}
