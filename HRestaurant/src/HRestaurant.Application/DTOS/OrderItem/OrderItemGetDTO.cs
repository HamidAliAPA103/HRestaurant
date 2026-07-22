namespace HRestaurant.DTOS.OrderItem
{
    public class OrderItemGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid OrderId { get; set; }
        public Guid MenuId { get; set; }
        public int Say { get; set; }
        public decimal Prices { get; set; }
    }
}
