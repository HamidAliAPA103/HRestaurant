namespace HRestaurant.DTOS.Public;

public sealed class PublicBranchDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;

    public bool IsOpenNow { get; set; }

    public IReadOnlyCollection<PublicWorkingHourDto> WorkingHours
    {
        get;
        set;
    } = Array.Empty<PublicWorkingHourDto>();
}
