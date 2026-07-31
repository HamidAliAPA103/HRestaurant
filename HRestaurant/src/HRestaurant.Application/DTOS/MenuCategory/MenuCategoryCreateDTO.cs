using System.Text.Json.Serialization;

namespace HRestaurant.DTOS.MenuCategory;

public sealed class MenuCategoryCreateDTO
{
    [JsonPropertyName("restaurantId")]
    public Guid ResdaranId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}
