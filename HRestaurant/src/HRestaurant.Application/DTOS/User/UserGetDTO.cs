namespace HRestaurant.DTOS.User
{
    public class UserGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid BranchId { get; set; }
        public Guid? AppUserId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateOnly? HireDate { get; set; }
        public string? AvatarUrl { get; set; }
        public string? EmergencyContact { get; set; }
        public bool IsActive { get; set; }
    }
}
