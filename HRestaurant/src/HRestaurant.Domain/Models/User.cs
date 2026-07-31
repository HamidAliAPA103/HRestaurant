using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class User : BaseEntity
    {
        public Guid? RestaurantId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? AppUserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NormalizedEmail { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? NormalizedPhone { get; set; }
        public string Role { get; set; } = "Customer";
        public decimal Salary { get; set; }
        public DateOnly? HireDate { get; set; }
        public string? AvatarUrl { get; set; }
        public string? EmergencyContact { get; set; }
        public bool IsActive { get; set; } = true;
        public Restaurant? Restaurant { get; set; }
        public Branch? Branch { get; set; }
        public List<EmployeeShift> EmployeeShifts { get; set; } = new();
        public List<Order> Orders { get; set; } = new ();
        public List<Order> WaiterOrders { get; set; } = new ();
        public List<Reservation> Reservations { get; set; } = new ();
        public List<Review> Reviews { get; set; } = new ();
    }
}
