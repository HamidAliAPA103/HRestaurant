namespace HRestaurant.DTOS.Restaurant;

public sealed class RestaurantSettingsUpdateDTO
{
    public string Currency { get; set; } = string.Empty;

    public decimal TaxRate { get; set; }
}
