using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Order : BaseEntity
    {
        public Guid CustomerID { get; set; }
        public Guid? TableID { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalPrices { get; set; }
        public User Customer { get; set; } = null!;
        public Table? Table { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}
