using HRestaurant.Models.BaseModels;

namespace HRestaurant.Models
{
    public class Review : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid ResdaranId { get; set; }
        public int Rating { get; set; }           
        public string? Comment { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
        public User Customer { get; set; } = null!;
    }
}
