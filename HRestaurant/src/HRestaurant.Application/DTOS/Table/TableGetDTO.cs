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
        public Guid? BranchId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int Tutum { get; set; }
        public TableStatus Status { get; set; }
        public TableShape Shape { get; set; }
        public double? PositionX { get; set; }
        public double? PositionY { get; set; }
        public double? PositionZ { get; set; }
        public double? RotationX { get; set; }
        public double? RotationY { get; set; }
        public double? RotationZ { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public bool IsActive { get; set; }
    }
}
