using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models;

public sealed class Branch : BaseEntity
{
    public Guid RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public Guid? ManagerId { get; set; }

    public string TimeZoneId { get; set; } = "Asia/Baku";

    public bool IsActive { get; set; } = true;

    public Restaurant Restaurant { get; set; } = null!;

    public List<BranchWorkingHour> WorkingHours { get; set; } = new();

    public List<Table> Tables { get; set; } = new();

    public List<Reservation> Reservations { get; set; } = new();

    public List<User> Employees { get; set; } = new();

        public List<Shift> Shifts { get; set; } = new();
        public List<InventoryItem> InventoryItems { get; set; } = new();
        public List<InventoryNotification> InventoryNotifications { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
}
