using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Menu : BaseEntity
    {
        public Guid RestaurantId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsPopular { get; set; }
        public string Desc {  get; set; } = string.Empty;
        public string Nutrition { get; set; } = string.Empty;
        public string? Model3DUrl { get; set; }
        public string? ModelPosterUrl { get; set; }
        public decimal ModelScale { get; set; } = 1m;
        public decimal ModelRotationX { get; set; }
        public decimal ModelRotationY { get; set; }
        public decimal ModelRotationZ { get; set; }
        public bool Is3DEnabled { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
        public MenuCategory Category { get; set; } = null!;
        public List<MenuItemIngredient> Ingredients { get; set; } = new();

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
