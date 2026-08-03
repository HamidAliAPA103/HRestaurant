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

    public string? Model3DUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public decimal? Calories { get; set; }

    public decimal? Protein { get; set; }

    public decimal? Carbohydrates { get; set; }

    public decimal? Fat { get; set; }

    public string? Origin { get; set; }

    public string? AllergenInformation { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public List<MenuItemIngredient> MenuItems { get; set; } = new();

    public List<InventoryItem> InventoryItems { get; set; } = new();
}
