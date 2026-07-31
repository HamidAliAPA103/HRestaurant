namespace HRestaurant.DTOS.MenuCategory
{
    public class MenuCategoryUpdateDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
