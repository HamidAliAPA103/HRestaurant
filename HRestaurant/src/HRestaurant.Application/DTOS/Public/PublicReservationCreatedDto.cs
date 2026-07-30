namespace HRestaurant.DTOS.Public;

public sealed class PublicReservationCreatedDto
{
    public Guid ReservationId { get; init; }

    public string ConfirmationCode { get; init; } = string.Empty;

    public string TrackingToken { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string RestaurantName { get; init; } = string.Empty;

    public string BranchName { get; init; } = string.Empty;

    public string TableNumber { get; init; } = string.Empty;

    public DateOnly ReservationDate { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    public bool EmailDeliveryQueued { get; init; }
}
