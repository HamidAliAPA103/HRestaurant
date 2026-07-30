namespace HRestaurant.DTOS.Restaurant;

public sealed class RestaurantWorkingHourDTO
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly? OpensAt { get; set; }

    public TimeOnly? ClosesAt { get; set; }

    public bool IsClosed { get; set; }
}
