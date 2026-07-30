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
        public Guid? CustomerId { get; set; }
        public Guid BranchId { get; set; }
        public Guid TableId { get; set; }
        public DateTime ReservationTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public int GuestCount { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ConfirmationCode { get; set; } = string.Empty;
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }
}
