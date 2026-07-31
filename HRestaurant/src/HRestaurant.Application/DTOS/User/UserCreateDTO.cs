namespace HRestaurant.DTOS.User
{
    public class UserCreateDTO
    {
        public Guid RestaurantId { get; set; }
        public Guid BranchId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateOnly HireDate { get; set; }
        public string? AvatarUrl { get; set; }
        public string EmergencyContact { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
