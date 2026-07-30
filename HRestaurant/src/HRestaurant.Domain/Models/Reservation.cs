using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Reservation : BaseEntity
    {
        public Guid? CustomerId { get; set; }
        public Guid BranchId { get; set; }
        public Guid TableId { get; set; }
        public DateTime ReservationTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; } = 120;
        public int GuestCount { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNormalized { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? SpecialNotes { get; set; }
        public string ConfirmationCode { get; set; } = string.Empty;
        public string PublicTrackingTokenHash { get; set; } = string.Empty;
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public Table Table { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
        public User? Customer { get; set; }
        public List<ReservationAuditLog> AuditLogs { get; set; } = new();
    }
}
