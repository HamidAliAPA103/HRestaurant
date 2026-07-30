namespace HRestaurant.DTOS.Public;

public sealed class PublicCreateReservationDto
{
    public Guid BranchId { get; init; }

    public Guid TableId { get; init; }

    public DateOnly ReservationDate { get; init; }

    public TimeOnly StartTime { get; init; }

    public int DurationMinutes { get; init; } = 120;

    public int GuestCount { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string? Email { get; init; }

    public string? SpecialNotes { get; init; }

    public bool TermsAccepted { get; init; }

    public string? CaptchaToken { get; init; }
}
