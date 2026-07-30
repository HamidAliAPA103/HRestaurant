namespace HRestaurant.DTOS.Public;

public sealed class PublicWorkingHourDto
{
    public DayOfWeek DayOfWeek { get; set; }

    public string DayName { get; set; } = string.Empty;

    public TimeOnly? OpensAt { get; set; }

    public TimeOnly? ClosesAt { get; set; }

    public bool IsClosed { get; set; }
}
