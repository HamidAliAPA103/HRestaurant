namespace HRestaurant.DTOS.Restaurant
{
    public class RestaurantGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; }
        public string Name { get; set; }
        public string Adres { get; set; }
        public string Number { get; set; }
        public bool IsDeleted { get; set; }
    }
}
