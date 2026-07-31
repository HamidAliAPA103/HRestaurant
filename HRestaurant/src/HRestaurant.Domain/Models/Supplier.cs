using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class Supplier : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Restaurant Restaurant { get; set; } = null!;
    public List<InventoryItem> InventoryItems { get; set; } = [];
}
