using HRestaurant.Enum;

namespace HRestaurant.DTOS.Reservation
{
    public class ReservationUpdateDTO
    {
        public Guid CustomerId { get; set; }
        public Guid TableId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int GuestCount { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }
}
