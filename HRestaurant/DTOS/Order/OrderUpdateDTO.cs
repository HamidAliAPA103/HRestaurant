using HRestaurant.Enum;

namespace HRestaurant.DTOS.Order
{
    public class OrderUpdateDTO
    {
        public Guid? TableID { get; set; }
        public OrderStatus Status { get; set; }
    }
}
