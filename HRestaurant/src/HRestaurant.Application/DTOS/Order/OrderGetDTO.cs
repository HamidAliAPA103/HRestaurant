using HRestaurant.Enum;

namespace HRestaurant.DTOS.Order
{
    public class OrderGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CustomerID { get; set; }
        public Guid? TableID { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalPrices { get; set; }
    }
}
