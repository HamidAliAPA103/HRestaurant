namespace HRestaurant.DTOS.Menu;

public sealed class MenuItemIngredientDTO
{
    public Guid IngredientId { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal ExplodedPositionX { get; set; }
    public decimal ExplodedPositionY { get; set; }
    public decimal ExplodedPositionZ { get; set; }
    public decimal ExplodedRotationX { get; set; }
    public decimal ExplodedRotationY { get; set; }
    public decimal ExplodedRotationZ { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisibleIn3D { get; set; } = true;
}
