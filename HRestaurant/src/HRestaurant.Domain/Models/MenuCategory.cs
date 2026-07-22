using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class MenuCategory : BaseEntity
    {
        public Guid ResdaranId { get; set; }
        public string Name { get; set; } = null!;
        public Restaurant Restaurant { get; set; } = null!;
        public List<Menu> Menus { get; set; } = new ();
    }
}
