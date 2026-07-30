namespace HRestaurant.DTOS.Public;

public sealed class TableAvailabilityRequestDto
{
    public DateOnly ReservationDate { get; init; }

    public TimeOnly StartTime { get; init; }

    public int GuestCount { get; init; }

    public int DurationMinutes { get; init; } = 120;
}
