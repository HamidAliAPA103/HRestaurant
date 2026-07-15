namespace HRestaurant.DTOS.MenuCategory
{
    public class MenuCategoryGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid ResdaranId { get; set; }
        public string Name { get; set; } = null!;
    }
}
