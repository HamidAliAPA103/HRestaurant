using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Restaurant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Currency { get; set; } = "AZN";
        public decimal TaxRate { get; set; }
        public List<Table> Tables { get; set; } = new();
        public List<MenuCategory> Categories { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<RestaurantWorkingHour> WorkingHours { get; set; } =
            new();
    }
}
