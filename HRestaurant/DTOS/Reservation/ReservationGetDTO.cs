using HRestaurant.Enum;

namespace HRestaurant.DTOS.Reservation
{
    public class ReservationGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CustomerId { get; set; }
        public Guid TableId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int GuestCount { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }
}
