using HRestaurant.DTOS.MenuCategory;

namespace HRestaurant.Services.Interfaces;

public interface IMenuCategoryService :
    ICrudService<
        MenuCategoryCreateDTO,
        MenuCategoryUpdateDTO,
        MenuCategoryGetDTO>;
