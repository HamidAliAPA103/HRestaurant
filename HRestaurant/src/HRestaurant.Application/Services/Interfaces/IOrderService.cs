using HRestaurant.DTOS.Order;

namespace HRestaurant.Services.Interfaces;

public interface IOrderService :
    ICrudService<OrderCreatDTO, OrderUpdateDTO, OrderGetDTO>;
