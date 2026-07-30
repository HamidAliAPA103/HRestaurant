namespace HRestaurant.DTOS.Public;

public sealed class PublicCancelReservationDto
{
    public string? Phone { get; init; }

    public string? TrackingToken { get; init; }

    public string? Reason { get; init; }

    public string? CaptchaToken { get; init; }
}
