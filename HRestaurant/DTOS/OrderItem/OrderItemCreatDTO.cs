namespace HRestaurant.DTOS.OrderItem
{
    public class OrderItemCreatDTO
    {
        public Guid OrderId { get; set; }
        public Guid MenuId { get; set; }
        public int Say { get; set; }
        public decimal Prices { get; set; }
    }
}
