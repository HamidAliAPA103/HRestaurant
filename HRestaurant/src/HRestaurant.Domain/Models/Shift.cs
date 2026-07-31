using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class Shift : BaseEntity
{
    public Guid RestaurantId { get; set; }

    public Guid BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsActive { get; set; } = true;

    public Restaurant Restaurant { get; set; } = null!;

    public Branch Branch { get; set; } = null!;

    public List<EmployeeShift> EmployeeShifts { get; set; } = new();
}
