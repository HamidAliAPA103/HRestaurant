using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Table : BaseEntity
    {
        public Guid RestaurantID { get; set; }
        public int Tutum { get; set; }
        public TableStatus Status { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
        public List<Order> Orders { get; set; } = new();
        public List<Reservation> Reservations { get; set; } = new();
        
    }
}
