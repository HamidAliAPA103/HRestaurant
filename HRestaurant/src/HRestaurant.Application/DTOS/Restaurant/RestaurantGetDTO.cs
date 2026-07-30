namespace HRestaurant.DTOS.Restaurant;

public sealed class RestaurantGetDTO
{
    public Guid ID { get; set; }

    public DateTime CreatAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Adres { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string Currency { get; set; } = string.Empty;

    public decimal TaxRate { get; set; }

    public bool IsDeleted { get; set; }

    public List<RestaurantWorkingHourDTO> WorkingHours { get; set; } = [];
}
