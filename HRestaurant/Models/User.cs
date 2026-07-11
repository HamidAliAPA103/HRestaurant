using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = "Customer";
        public List<Order> Orders { get; set; } = new ();
        public List<Reservation> Reservations { get; set; } = new ();
        public List<Review> Reviews { get; set; } = new ();
    }
}
