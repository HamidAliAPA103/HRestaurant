using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class BranchWorkingHour : BaseEntity
{
    public Guid BranchId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly? OpensAt { get; set; }

    public TimeOnly? ClosesAt { get; set; }

    public bool IsClosed { get; set; }

    public Branch Branch { get; set; } = null!;
}
