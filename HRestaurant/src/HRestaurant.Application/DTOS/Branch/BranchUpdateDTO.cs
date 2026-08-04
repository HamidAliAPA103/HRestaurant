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
    public string? FrontImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ShortDescription { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public string? VirtualTourUrl { get; set; }
    public string? ParkingInfo { get; set; }
    public string? Landmark { get; set; }
    public bool IsPubliclyVisible { get; set; } = true;

    public string TimeZoneId { get; set; } = "Asia/Baku";
}
