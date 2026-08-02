using AutoMapper;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Restaurants;

public sealed class RestaurantProfile : Profile
{
    public RestaurantProfile()
    {
        CreateMap<RestaurantWorkingHour, RestaurantWorkingHourDTO>();

        CreateMap<RestaurantWorkingHourDTO, RestaurantWorkingHour>()
            .IgnoreBaseEntityMembers()
            .ForMember(
                destination => destination.RestaurantId,
                options => options.Ignore())
            .ForMember(
                destination => destination.Restaurant,
                options => options.Ignore());

        CreateMap<Restaurant, RestaurantGetDTO>();

        CreateMap<RestaurantCreateDTO, Restaurant>()
            .IgnoreBaseEntityMembers()
            .ForMember(
                destination => destination.IsActive,
                options => options.MapFrom(_ => true))
            .ForMember(
                destination => destination.Branches,
                options => options.Ignore())
            .ForMember(
                destination => destination.Tables,
                options => options.Ignore())
            .ForMember(
                destination => destination.Categories,
                options => options.Ignore())
            .ForMember(
                destination => destination.Reviews,
                options => options.Ignore())
            .ForMember(destination => destination.Employees, options => options.Ignore())
            .ForMember(destination => destination.Shifts, options => options.Ignore())
            .ForMember(destination => destination.Ingredients, options => options.Ignore())
            .ForMember(destination => destination.Suppliers, options => options.Ignore())
            .ForMember(destination => destination.InventoryItems, options => options.Ignore())
            .ForMember(destination => destination.InventoryNotifications, options => options.Ignore())
            .ForMember(destination => destination.Orders, options => options.Ignore())
            .ForMember(destination => destination.Payments, options => options.Ignore());

#pragma warning disable CS0618
        CreateMap<RestaurantCreatDTO, Restaurant>()
            .IncludeBase<RestaurantCreateDTO, Restaurant>();
#pragma warning restore CS0618

        CreateMap<RestaurantUpdateDTO, Restaurant>()
            .IgnoreBaseEntityMembers()
            .ForMember(
                destination => destination.IsActive,
                options => options.Ignore())
            .ForMember(
                destination => destination.Slug,
                options => options.Ignore())
            .ForMember(
                destination => destination.Currency,
                options => options.Ignore())
            .ForMember(
                destination => destination.TaxRate,
                options => options.Ignore())
            .ForMember(
                destination => destination.Tables,
                options => options.Ignore())
            .ForMember(
                destination => destination.Branches,
                options => options.Ignore())
            .ForMember(
                destination => destination.Categories,
                options => options.Ignore())
            .ForMember(
                destination => destination.Reviews,
                options => options.Ignore())
            .ForMember(
                destination => destination.WorkingHours,
                options => options.Ignore())
            .ForMember(destination => destination.Employees, options => options.Ignore())
            .ForMember(destination => destination.Shifts, options => options.Ignore())
            .ForMember(destination => destination.Ingredients, options => options.Ignore())
            .ForMember(destination => destination.Suppliers, options => options.Ignore())
            .ForMember(destination => destination.InventoryItems, options => options.Ignore())
            .ForMember(destination => destination.InventoryNotifications, options => options.Ignore())
            .ForMember(destination => destination.Orders, options => options.Ignore())
            .ForMember(destination => destination.Payments, options => options.Ignore())
            .IgnoreNullSourceMembers();
    }
}
