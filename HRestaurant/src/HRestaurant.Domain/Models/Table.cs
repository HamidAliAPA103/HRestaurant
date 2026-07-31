using HRestaurant.Enum;
using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Table : BaseEntity
    {
        public Guid RestaurantID { get; set; }
        public Guid? BranchId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int Tutum { get; set; }
        public TableStatus Status { get; set; }
        public TableShape Shape { get; set; } = TableShape.Round;
        public double? PositionX { get; set; }
        public double? PositionY { get; set; }
        public double? PositionZ { get; set; }
        public double? RotationX { get; set; }
        public double? RotationY { get; set; }
        public double? RotationZ { get; set; }
        public double Width { get; set; } = 1.8;
        public double Length { get; set; } = 1.8;
        public double Height { get; set; } = 0.75;
        public bool IsActive { get; set; } = true;
        public Restaurant Restaurant { get; set; } = null!;
        public Branch? Branch { get; set; }
        public List<Order> Orders { get; set; } = new();
        public List<Reservation> Reservations { get; set; } = new();
    }
}
