using HRestaurant.Enum;

namespace HRestaurant.DTOS.Table
{
    public class TableGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid RestaurantID { get; set; }
        public int Tutum { get; set; }
        public TableStatus Status { get; set; }
    }
}
