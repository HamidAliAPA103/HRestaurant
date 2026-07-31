using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class EmployeeShift : BaseEntity
{
    public Guid EmployeeId { get; set; }

    public Guid ShiftId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Notes { get; set; }

    public EmployeeShiftStatus Status { get; set; } =
        EmployeeShiftStatus.Scheduled;

    public User Employee { get; set; } = null!;

    public Shift Shift { get; set; } = null!;
}
