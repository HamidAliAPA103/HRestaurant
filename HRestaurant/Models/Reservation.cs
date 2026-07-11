using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Reservation : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid TableId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int GuestCount { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public Table Table { get; set; } = null!;
        public User Customer { get; set; } = null!;
    }
}
