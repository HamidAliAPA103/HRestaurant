using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid MenuId { get; set; }
        public int Say { get; set; }         
        public decimal Prices { get; set; }
        public Order Order { get; set; } = null!;
        public Menu Menu { get; set; } = null!;
      
    }
}
