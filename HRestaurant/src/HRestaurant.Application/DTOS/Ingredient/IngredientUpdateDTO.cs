using HRestaurant.Enum;

namespace HRestaurant.DTOS.Ingredient;

public sealed class IngredientUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; }
    public bool IsActive { get; set; }
}
