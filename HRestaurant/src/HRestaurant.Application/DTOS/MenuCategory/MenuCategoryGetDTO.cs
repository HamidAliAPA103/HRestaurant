using System.Text.Json.Serialization;

namespace HRestaurant.DTOS.MenuCategory;

public sealed class MenuCategoryGetDTO
{
    public Guid ID { get; set; }
    public DateTime CreatAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    [JsonPropertyName("restaurantId")]
    public Guid ResdaranId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
