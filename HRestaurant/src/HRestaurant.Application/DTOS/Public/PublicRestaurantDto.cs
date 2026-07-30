namespace HRestaurant.DTOS.Public;

public sealed class PublicRestaurantDto
{
    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? Description { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Address { get; set; } = string.Empty;

    public bool IsOpenNow { get; set; }

    public IReadOnlyCollection<PublicWorkingHourDto> WorkingHours
    {
        get;
        set;
    } = Array.Empty<PublicWorkingHourDto>();

    public IReadOnlyCollection<PublicBranchDto> Branches
    {
        get;
        set;
    } = Array.Empty<PublicBranchDto>();
}
