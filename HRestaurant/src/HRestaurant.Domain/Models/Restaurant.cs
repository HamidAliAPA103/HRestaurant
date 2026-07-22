using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Restaurant : BaseEntity
    {
        public string Name { get; set; }
        public string Adres { get; set; }
        public string Number { get; set; }
        public List<Table> Tables { get; set; } = new();
        public List<MenuCategory> Categories { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();


    }
}
