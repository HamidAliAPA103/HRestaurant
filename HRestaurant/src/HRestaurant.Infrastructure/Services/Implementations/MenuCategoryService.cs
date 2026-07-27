using AutoMapper;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class MenuCategoryService :
    CrudServiceBase<
        MenuCategory,
        MenuCategoryCreateDTO,
        MenuCategoryUpdateDTO,
        MenuCategoryGetDTO>,
    IMenuCategoryService
{
    public MenuCategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper, "Menu category")
    {
    }
}
