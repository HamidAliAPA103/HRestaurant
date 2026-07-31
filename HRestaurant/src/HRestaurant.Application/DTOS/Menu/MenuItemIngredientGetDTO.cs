using HRestaurant.Enum;

namespace HRestaurant.DTOS.Menu;

public sealed class MenuItemIngredientGetDTO
{
    public Guid IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; }
    public decimal RequiredQuantity { get; set; }
}
