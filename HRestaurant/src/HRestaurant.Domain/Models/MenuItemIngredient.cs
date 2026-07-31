namespace HRestaurant.Models;

public sealed class MenuItemIngredient
{
    public Guid MenuItemId { get; set; }

    public Guid IngredientId { get; set; }

    public decimal RequiredQuantity { get; set; }

    public Menu MenuItem { get; set; } = null!;

    public Ingredient Ingredient { get; set; } = null!;
}
