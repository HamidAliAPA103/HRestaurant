using HRestaurant.Models;

namespace HRestaurant.DTOS.Menu
{
    public class MenuCreateDTO
    {
        public IFormFile Image { get; set; }
        public decimal Price { get; set; }
        public string Desc { get; set; }
        public Guid CategoryId { get; set; }
        public string Nutrition { get; set; }
    }
}
