using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class RestaurantWorkingHour : BaseEntity
{
    public Guid RestaurantId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly? OpensAt { get; set; }

    public TimeOnly? ClosesAt { get; set; }

    public bool IsClosed { get; set; }

    public Restaurant Restaurant { get; set; } = null!;
}
