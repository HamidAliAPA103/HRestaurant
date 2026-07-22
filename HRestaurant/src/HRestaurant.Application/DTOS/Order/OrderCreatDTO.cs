using HRestaurant.DTOS.OrderItem;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Order
{
    public class OrderCreatDTO
    {
        public Guid CustomerID { get; set; }
        public Guid? TableID { get; set; }
        public List<OrderItemCreatDTO> Items { get; set; } = new();

    }
}
