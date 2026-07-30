using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class ReservationAuditLog : BaseEntity
{
    public Guid ReservationId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public string? IpAddressHash { get; set; }

    public Reservation Reservation { get; set; } = null!;
}
