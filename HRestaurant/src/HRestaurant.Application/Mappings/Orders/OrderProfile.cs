using AutoMapper;
using HRestaurant.DTOS.Order;
using HRestaurant.Mappings.Common;
using HRestaurant.Models;

namespace HRestaurant.Mappings.Orders;

public sealed class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderGetDTO>()
            .ForMember(x => x.BranchName, o => o.MapFrom(x => x.Branch.Name))
            .ForMember(x => x.TableNumber,
                o => o.MapFrom(x => x.Table == null ? null : x.Table.TableNumber))
            .ForMember(x => x.WaiterName,
                o => o.MapFrom(x => x.Waiter == null ? null : x.Waiter.Name))
            .ForMember(x => x.CustomerName,
                o => o.MapFrom(x => x.Customer == null ? null : x.Customer.Name));

        CreateMap<OrderCreatDTO, Order>()
            .IgnoreBaseEntityMembers()
            .ForMember(x => x.Status, o => o.Ignore())
            .ForMember(x => x.OrderNumber, o => o.Ignore())
            .ForMember(x => x.Subtotal, o => o.Ignore())
            .ForMember(x => x.DiscountAmount, o => o.Ignore())
            .ForMember(x => x.OrderDiscountPercentage,
                o => o.MapFrom(x => x.DiscountPercentage))
            .ForMember(x => x.TaxAmount, o => o.Ignore())
            .ForMember(x => x.TotalAmount, o => o.Ignore())
            .ForMember(x => x.PreparingAt, o => o.Ignore())
            .ForMember(x => x.ReadyAt, o => o.Ignore())
            .ForMember(x => x.CompletedAt, o => o.Ignore())
            .ForMember(x => x.CancelledAt, o => o.Ignore())
            .ForMember(x => x.CancelReason, o => o.Ignore())
            .ForMember(x => x.InventoryConsumedAt, o => o.Ignore())
            .ForMember(x => x.InventoryReturnedAt, o => o.Ignore())
            .ForMember(x => x.IsPaid, o => o.Ignore())
            .ForMember(x => x.RefundRequired, o => o.Ignore())
            .ForMember(x => x.RefundedAt, o => o.Ignore())
            .ForMember(x => x.RowVersion, o => o.Ignore())
            .ForMember(x => x.Restaurant, o => o.Ignore())
            .ForMember(x => x.Branch, o => o.Ignore())
            .ForMember(x => x.Customer, o => o.Ignore())
            .ForMember(x => x.Waiter, o => o.Ignore())
            .ForMember(x => x.Table, o => o.Ignore())
            .ForMember(x => x.Items, o => o.Ignore());

        CreateMap<OrderUpdateDTO, Order>()
            .IgnoreBaseEntityMembers()
            .ForAllMembers(options => options.Ignore());
    }
}
