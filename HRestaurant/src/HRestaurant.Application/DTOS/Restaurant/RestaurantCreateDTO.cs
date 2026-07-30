namespace HRestaurant.DTOS.Restaurant;

public class RestaurantCreateDTO
{
    public string Name { get; set; } = string.Empty;

    public string Adres { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public string Currency { get; set; } = "AZN";

    public decimal TaxRate { get; set; }

    public List<RestaurantWorkingHourDTO> WorkingHours { get; set; } = [];
}
