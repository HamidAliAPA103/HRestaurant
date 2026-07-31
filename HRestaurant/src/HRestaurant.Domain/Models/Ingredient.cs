using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class Ingredient : BaseEntity
{
    public Guid RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public IngredientUnit Unit { get; set; }

    public bool IsActive { get; set; } = true;

    public Restaurant Restaurant { get; set; } = null!;

    public List<MenuItemIngredient> MenuItems { get; set; } = new();
}
