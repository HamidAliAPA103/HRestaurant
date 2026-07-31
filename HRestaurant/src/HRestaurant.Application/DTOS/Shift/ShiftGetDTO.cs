namespace HRestaurant.DTOS.Shift;

public sealed class ShiftGetDTO
{
    public Guid ID { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}
