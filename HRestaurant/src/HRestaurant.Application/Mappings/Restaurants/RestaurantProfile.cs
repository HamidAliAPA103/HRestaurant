using AutoMapper;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Restaurants;

public sealed class RestaurantProfile : Profile
{
    public RestaurantProfile()
    {
        CreateMap<Restaurant, RestaurantGetDTO>();

        CreateMap<RestaurantCreatDTO, Restaurant>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Tables, options => options.Ignore())
            .ForMember(destination => destination.Categories, options => options.Ignore())
            .ForMember(destination => destination.Reviews, options => options.Ignore());

        CreateMap<RestaurantUpdateDTO, Restaurant>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Tables, options => options.Ignore())
            .ForMember(destination => destination.Categories, options => options.Ignore())
            .ForMember(destination => destination.Reviews, options => options.Ignore())
            .IgnoreNullSourceMembers();
    }
}
