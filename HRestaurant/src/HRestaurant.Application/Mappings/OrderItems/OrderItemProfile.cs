using AutoMapper;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.OrderItems;

public sealed class OrderItemProfile : Profile
{
    public OrderItemProfile()
    {
        CreateMap<OrderItem, OrderItemGetDTO>();

        CreateMap<OrderItemCreatDTO, OrderItem>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.Order, options => options.Ignore())
            .ForMember(destination => destination.Menu, options => options.Ignore());

        CreateMap<OrderItemUpdateDTO, OrderItem>()
            .IgnoreBaseEntityMembers()
            .ForMember(destination => destination.OrderId, options => options.Ignore())
            .ForMember(destination => destination.MenuId, options => options.Ignore())
            .ForMember(destination => destination.Order, options => options.Ignore())
            .ForMember(destination => destination.Menu, options => options.Ignore());
    }
}
