using AutoMapper;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class OrderService :
    CrudServiceBase<
        Order,
        OrderCreatDTO,
        OrderUpdateDTO,
        OrderGetDTO>,
    IOrderService
{
    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(unitOfWork, mapper, "Order")
    {
    }

    public override async Task<ApiResponse<Guid>> CreateAsync(
        OrderCreatDTO dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var order = Mapper.Map<Order>(dto);
        order.TotalPrices = 0;

        for (var index = 0; index < dto.Items.Count; index++)
        {
            var itemDto = dto.Items[index];
            var menuItem = await UnitOfWork.MenuItems.GetByIdAsync(
                itemDto.MenuId,
                cancellationToken);

            if (menuItem is null || menuItem.IsDeleted)
            {
                return ApiResponse.NotFound<Guid>("Menu item");
            }

            var orderItem = order.Items[index];
            orderItem.OrderId = Guid.Empty;
            orderItem.Prices = menuItem.Price;
            order.TotalPrices += orderItem.Prices * orderItem.Say;
        }

        await UnitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await Repository.AddAsync(order, cancellationToken);
            var saveCount = await UnitOfWork.SaveChangesAsync(
                cancellationToken);

            if (saveCount <= 0)
            {
                await UnitOfWork.RollbackTransactionAsync(
                    CancellationToken.None);

                return ApiResponse.PersistenceFailure<Guid>();
            }
        }
        catch (Exception operationException)
        {
            try
            {
                await UnitOfWork.RollbackTransactionAsync(
                    CancellationToken.None);
            }
            catch (Exception rollbackException)
            {
                throw new AggregateException(
                    "Creating the order and rolling back the transaction both failed.",
                    operationException,
                    rollbackException);
            }

            throw;
        }

        await UnitOfWork.CommitTransactionAsync(cancellationToken);

        return ApiResponse.Created(
            order.ID,
            "Order created successfully.");
    }
}
