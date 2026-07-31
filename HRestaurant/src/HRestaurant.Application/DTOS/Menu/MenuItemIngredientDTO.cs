namespace HRestaurant.DTOS.Menu;

public sealed class MenuItemIngredientDTO
{
    public Guid IngredientId { get; set; }
    public decimal RequiredQuantity { get; set; }
}
