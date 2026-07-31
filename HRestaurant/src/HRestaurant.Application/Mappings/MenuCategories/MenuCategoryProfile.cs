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
            .ForMember(x => x.NormalizedName, o => o.Ignore())
            .ForMember(x => x.IsActive, o => o.MapFrom(_ => true))
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Menus, o => o.Ignore());

        CreateMap<MenuCategoryUpdateDTO, MenuCategory>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.ResdaranId, o => o.Ignore())
            .ForMember(x => x.NormalizedName, o => o.Ignore())
            .ForMember(x => x.IsActive, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Menus, o => o.Ignore())
            .IgnoreNullSourceMembers();
    }
}
