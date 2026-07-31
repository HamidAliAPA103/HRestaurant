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
            .ForMember(x => x.MenuItemId, o => o.MapFrom(x => x.MenuItemId))
            .ForMember(x => x.MenuItemName, o => o.Ignore())
            .ForMember(x => x.UnitPrice, o => o.Ignore())
            .ForMember(x => x.DiscountAmount, o => o.Ignore())
            .ForMember(x => x.TotalPrice, o => o.Ignore())
            .ForMember(x => x.Status, o => o.Ignore())
            .ForMember(x => x.Order, o => o.Ignore())
            .ForMember(x => x.MenuItem, o => o.Ignore());
        CreateMap<OrderItemUpdateDTO, OrderItem>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.OrderId, o => o.Ignore())
            .ForMember(x => x.MenuItemId, o => o.Ignore())
            .ForMember(x => x.MenuItemName, o => o.Ignore())
            .ForMember(x => x.UnitPrice, o => o.Ignore())
            .ForMember(x => x.DiscountAmount, o => o.Ignore())
            .ForMember(x => x.TotalPrice, o => o.Ignore())
            .ForMember(x => x.KitchenNote, o => o.Ignore())
            .ForMember(x => x.Status, o => o.Ignore())
            .ForMember(x => x.Order, o => o.Ignore())
            .ForMember(x => x.MenuItem, o => o.Ignore());
    }
}
