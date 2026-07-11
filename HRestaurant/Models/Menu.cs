using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Menu : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public string Image { get; set; }
        public string ImageURL { get; set; }
        public decimal Price { get; set; }
        public string Desc {  get; set; }
        public string Nutrition { get; set; }
        public MenuCategory Category { get; set; } = null!;

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
