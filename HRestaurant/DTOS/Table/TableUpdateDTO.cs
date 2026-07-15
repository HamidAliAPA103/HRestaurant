using HRestaurant.Enum;

namespace HRestaurant.DTOS.Table
{
    public class TableUpdateDTO
    {
        public Guid RestaurantID { get; set; }
        public int Tutum { get; set; }
        public TableStatus Status { get; set; }
    }
}
