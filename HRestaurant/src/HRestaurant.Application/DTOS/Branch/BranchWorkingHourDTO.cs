namespace HRestaurant.DTOS.Branch;

public sealed class BranchWorkingHourDTO
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly? OpensAt { get; set; }

    public TimeOnly? ClosesAt { get; set; }

    public bool IsClosed { get; set; }
}
