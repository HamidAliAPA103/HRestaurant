namespace HRestaurant.DTOS.Public;

public sealed class PublicReservationDetailsDto
{
    public string ConfirmationCode { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string RestaurantName { get; init; } = string.Empty;

    public string BranchName { get; init; } = string.Empty;

    public string BranchAddress { get; init; } = string.Empty;

    public DateOnly ReservationDate { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    public int GuestCount { get; init; }

    public string TableNumber { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string MaskedPhone { get; init; } = string.Empty;

    public string? MaskedEmail { get; init; }

    public string? SpecialNotes { get; init; }

    public bool CanCancel { get; init; }

    public DateTime? CancelledAt { get; init; }
}
