using HRestaurant.Enum;

namespace HRestaurant.DTOS.Shift;

public sealed class EmployeeShiftGetDTO
{
    public Guid ID { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateOnly WorkDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Notes { get; set; }
    public EmployeeShiftStatus Status { get; set; }
}
