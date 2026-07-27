using AutoMapper;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class OrderItemService :
    CrudServiceBase<
        OrderItem,
        OrderItemCreatDTO,
        OrderItemUpdateDTO,
        OrderItemGetDTO>,
    IOrderItemService
{
    public OrderItemService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper, "Order item")
    {
    }
}
