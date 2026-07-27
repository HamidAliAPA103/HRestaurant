using HRestaurant.DTOS.OrderItem;

namespace HRestaurant.Services.Interfaces;

public interface IOrderItemService :
    ICrudService<
        OrderItemCreatDTO,
        OrderItemUpdateDTO,
        OrderItemGetDTO>;
