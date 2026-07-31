using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class MenuCategory : BaseEntity
    {
        public Guid ResdaranId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public Restaurant Restaurant { get; set; } = null!;
        public List<Menu> Menus { get; set; } = new ();
    }
}
