using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Restaurant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public string Currency { get; set; } = "AZN";
        public decimal TaxRate { get; set; }
        public List<Branch> Branches { get; set; } = new();
        public List<Table> Tables { get; set; } = new();
        public List<MenuCategory> Categories { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<User> Employees { get; set; } = new();
        public List<Shift> Shifts { get; set; } = new();
        public List<Ingredient> Ingredients { get; set; } = new();
        public List<Supplier> Suppliers { get; set; } = new();
        public List<InventoryItem> InventoryItems { get; set; } = new();
        public List<InventoryNotification> InventoryNotifications { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<RestaurantWorkingHour> WorkingHours { get; set; } =
            new();
    }
}
