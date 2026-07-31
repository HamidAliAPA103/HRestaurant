namespace HRestaurant.DTOS.User
{
    public class UserUpdateDTO
    {
        public string? Email { get; set; } 
        public string? Name { get; set; } 
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public decimal? Salary { get; set; }
        public DateOnly? HireDate { get; set; }
        public string? AvatarUrl { get; set; }
        public string? EmergencyContact { get; set; }
    }
}
