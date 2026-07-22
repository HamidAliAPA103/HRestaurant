namespace HRestaurant.DTOS.Review
{
    public class ReviewCreateDTO
    {
        public Guid CustomerId { get; set; }
        public Guid ResdaranId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
