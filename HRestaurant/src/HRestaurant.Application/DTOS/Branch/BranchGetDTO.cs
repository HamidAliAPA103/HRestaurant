namespace HRestaurant.DTOS.Branch;

public sealed class BranchGetDTO
{
    public Guid ID { get; set; }

    public Guid RestaurantId { get; set; }

    public string RestaurantName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public Guid? ManagerId { get; set; }

    public string? ManagerName { get; set; }

    public string? ManagerEmail { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public List<BranchWorkingHourDTO> WorkingHours { get; set; } = [];
}
