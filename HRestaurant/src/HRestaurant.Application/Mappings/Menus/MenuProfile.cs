using AutoMapper;
using HRestaurant.DTOS.Menu;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Menus;

public sealed class MenuProfile : Profile
{
    public MenuProfile()
    {
        CreateMap<MenuItemIngredient, MenuItemIngredientGetDTO>()
            .ForMember(x => x.IngredientId, o => o.MapFrom(x => x.IngredientId))
            .ForMember(x => x.Name, o => o.MapFrom(x => x.Ingredient.Name))
            .ForMember(x => x.Unit, o => o.MapFrom(x => x.Ingredient.Unit));

        CreateMap<Menu, MenuGetDTO>()
            .ForMember(x => x.CategoryName, o => o.MapFrom(x => x.Category.Name));

        CreateMap<MenuCreateDTO, Menu>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantId, o => o.Ignore())
            .ForMember(x => x.NormalizedName, o => o.Ignore())
            .ForMember(x => x.Image, o => o.Ignore())
            .ForMember(x => x.ImageURL, o => o.Ignore())
            .ForMember(x => x.FinalPrice, o => o.Ignore())
            .ForMember(x => x.IsAvailable, o => o.MapFrom(_ => true))
            .ForMember(x => x.IsPopular, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Category, o => o.Ignore())
            .ForMember(x => x.Ingredients, o => o.Ignore())
            .ForMember(x => x.OrderItems, o => o.Ignore());

        CreateMap<MenuUpdateDTO, Menu>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantId, o => o.Ignore())
            .ForMember(x => x.CategoryId, o => o.Ignore())
            .ForMember(x => x.NormalizedName, o => o.Ignore())
            .ForMember(x => x.Image, o => o.Ignore())
            .ForMember(x => x.FinalPrice, o => o.Ignore())
            .ForMember(x => x.IsAvailable, o => o.Ignore())
            .ForMember(x => x.IsPopular, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Category, o => o.Ignore())
            .ForMember(x => x.Ingredients, o => o.Ignore())
            .ForMember(x => x.OrderItems, o => o.Ignore())
            .IgnoreNullSourceMembers();
    }
}
