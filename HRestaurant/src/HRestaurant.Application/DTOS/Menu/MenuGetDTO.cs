namespace HRestaurant.DTOS.Menu
{
    public class MenuGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CategoryId { get; set; }
        public string Image { get; set; }
        public string ImageURL { get; set; }
        public decimal Price { get; set; }
        public string Desc { get; set; }
        public string Nutrition { get; set; }
    }
}
