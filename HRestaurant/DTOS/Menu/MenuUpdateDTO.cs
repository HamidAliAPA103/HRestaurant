namespace HRestaurant.DTOS.Menu
{
    public class MenuUpdateDTO
    {
        public IFormFile? Image { get; set; }
        public string? ImageURL { get; set; }
        public decimal? Price { get; set; }
        public string? Desc { get; set; }
        public string? Nutrition { get; set; }
    }
}
