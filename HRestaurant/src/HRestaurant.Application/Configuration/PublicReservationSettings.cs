using HRestaurant.Enum;

namespace HRestaurant.Configuration;

public sealed class PublicReservationSettings
{
    public const string SectionName = "PublicReservations";

    public int DefaultDurationMinutes { get; set; } = 120;

    public int MinimumDurationMinutes { get; set; } = 30;

    public int MaximumDurationMinutes { get; set; } = 240;

    public int SlotIntervalMinutes { get; set; } = 30;

    public int BufferMinutes { get; set; } = 15;

    public int MinimumGuestCount { get; set; } = 1;

    public int MaximumGuestCount { get; set; } = 20;

    public int CancellationCutoffMinutes { get; set; } = 120;

    public ReservationStatus InitialStatus { get; set; } =
        ReservationStatus.Pending;

    public string PublicBaseUrl { get; set; } =
        "http://localhost:5173";
}
