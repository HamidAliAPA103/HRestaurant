namespace HRestaurant.DTOS.User
{
    public class UserGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; }
    }
}
