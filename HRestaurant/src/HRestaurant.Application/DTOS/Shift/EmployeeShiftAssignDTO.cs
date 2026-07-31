using HRestaurant.Enum;

namespace HRestaurant.DTOS.Shift;

public sealed class EmployeeShiftAssignDTO
{
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Notes { get; set; }
    public EmployeeShiftStatus Status { get; set; } =
        EmployeeShiftStatus.Scheduled;
}
