namespace HRestaurant.DTOS.Review
{
    public class ReviewGetDTO
    {
        public Guid ID { get; set; }
        public DateTime CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ResdaranId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
