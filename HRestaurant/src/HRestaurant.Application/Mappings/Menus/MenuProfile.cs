using AutoMapper;
using HRestaurant.DTOS.Menu;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Menus;

public sealed class MenuProfile : Profile
{
    public MenuProfile()
    {
        CreateMap<Menu, MenuGetDTO>();

        CreateMap<MenuCreateDTO, Menu>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Image, options => options.Ignore())
            .ForMember(destination => destination.ImageURL, options => options.Ignore())
            .ForMember(destination => destination.Category, options => options.Ignore())
            .ForMember(destination => destination.OrderItems, options => options.Ignore());

        CreateMap<MenuUpdateDTO, Menu>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.CategoryId, options => options.Ignore())
            .ForMember(destination => destination.Image, options => options.Ignore())
            .ForMember(destination => destination.Category, options => options.Ignore())
            .ForMember(destination => destination.OrderItems, options => options.Ignore())
            .IgnoreNullSourceMembers();
    }
}
