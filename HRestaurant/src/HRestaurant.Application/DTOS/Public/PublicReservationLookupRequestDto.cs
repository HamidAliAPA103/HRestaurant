namespace HRestaurant.DTOS.Public;

public sealed class PublicReservationLookupRequestDto
{
    public string? ConfirmationCode { get; init; }

    public string? Phone { get; init; }

    public string? TrackingToken { get; init; }

    public string? CaptchaToken { get; init; }
}
