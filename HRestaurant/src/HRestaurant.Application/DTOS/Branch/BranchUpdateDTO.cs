namespace HRestaurant.DTOS.Branch;

public sealed class BranchUpdateDTO
{
    public string Name { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string TimeZoneId { get; set; } = "Asia/Baku";
}
