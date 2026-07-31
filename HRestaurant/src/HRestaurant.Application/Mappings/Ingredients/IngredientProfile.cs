using AutoMapper;
using HRestaurant.DTOS.Ingredient;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Ingredients;

public sealed class IngredientProfile : Profile
{
    public IngredientProfile()
    {
        CreateMap<Ingredient, IngredientGetDTO>();
        CreateMap<IngredientCreateDTO, Ingredient>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.NormalizedName, o => o.Ignore())
            .ForMember(x => x.IsActive, o => o.MapFrom(_ => true))
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.MenuItems, o => o.Ignore());
        CreateMap<IngredientUpdateDTO, Ingredient>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.RestaurantId, o => o.Ignore())
            .ForMember(x => x.NormalizedName, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.MenuItems, o => o.Ignore());
    }
}
