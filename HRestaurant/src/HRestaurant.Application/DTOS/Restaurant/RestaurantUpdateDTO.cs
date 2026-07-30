namespace HRestaurant.DTOS.Restaurant;

public sealed class RestaurantUpdateDTO
{
    public string? Name { get; set; }

    public string? Adres { get; set; }

    public string? Number { get; set; }

    public string? Email { get; set; }

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? CoverImageUrl { get; set; }
}
