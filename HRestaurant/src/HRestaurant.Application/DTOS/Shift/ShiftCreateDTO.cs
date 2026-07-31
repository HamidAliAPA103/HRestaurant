namespace HRestaurant.DTOS.Shift;

public sealed class ShiftCreateDTO
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
