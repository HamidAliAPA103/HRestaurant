using AutoMapper;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.MenuCategories;

public sealed class MenuCategoryProfile : Profile
{
    public MenuCategoryProfile()
    {
        CreateMap<MenuCategory, MenuCategoryGetDTO>();

        CreateMap<MenuCategoryCreateDTO, MenuCategory>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Restaurant, options => options.Ignore())
            .ForMember(destination => destination.Menus, options => options.Ignore());

        CreateMap<MenuCategoryUpdateDTO, MenuCategory>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.ResdaranId, options => options.Ignore())
            .ForMember(destination => destination.Restaurant, options => options.Ignore())
            .ForMember(destination => destination.Menus, options => options.Ignore())
            .IgnoreNullSourceMembers();
    }
}
