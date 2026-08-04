namespace HRestaurant.DTOS.Public;

public sealed class PublicBranchDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

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
    public bool IsActive { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;

    public bool IsOpenNow { get; set; }

    public IReadOnlyCollection<PublicWorkingHourDto> WorkingHours
    {
        get;
        set;
    } = Array.Empty<PublicWorkingHourDto>();
}
