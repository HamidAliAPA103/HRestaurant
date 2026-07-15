namespace HRestaurant.DTOS.User
{
    public class UserCreateDTO
    {
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; }
    }
}
