using AutoMapper;
using HRestaurant.DTOS.Order;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Orders;

public sealed class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderGetDTO>();

        CreateMap<OrderCreatDTO, Order>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Status, options => options.Ignore())
            .ForMember(destination => destination.TotalPrices, options => options.Ignore())
            .ForMember(destination => destination.Customer, options => options.Ignore())
            .ForMember(destination => destination.Table, options => options.Ignore());

        CreateMap<OrderUpdateDTO, Order>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.CustomerID, options => options.Ignore())
            .ForMember(destination => destination.TotalPrices, options => options.Ignore())
            .ForMember(destination => destination.Customer, options => options.Ignore())
            .ForMember(destination => destination.Table, options => options.Ignore())
            .ForMember(destination => destination.Items, options => options.Ignore())
            .IgnoreNullSourceMembers();
    }
}
