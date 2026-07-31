using HRestaurant.Enum;

namespace HRestaurant.DTOS.Ingredient;

public sealed class IngredientGetDTO
{
    public Guid ID { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IngredientUnit Unit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}
