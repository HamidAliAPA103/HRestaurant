namespace HRestaurant.DTOS.Restaurant;

public class RestaurantCreateDTO
{
    public string Name { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string Adres { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public string Currency { get; set; } = "AZN";

    public decimal TaxRate { get; set; }

    public List<RestaurantWorkingHourDTO> WorkingHours { get; set; } = [];
}
