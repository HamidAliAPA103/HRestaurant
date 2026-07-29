using AutoMapper;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ApiResponse<object?>> UpdateKitchenStatusAsync(
        Guid id,
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        var order = await FindActiveOrderAsync(id, cancellationToken);

        if (order is null)
        {
            return ApiResponse.NotFound<object?>("Order");
        }

        var validTransition =
            order.Status == OrderStatus.Confirmed
                && status == OrderStatus.Preparing
            || order.Status == OrderStatus.Preparing
                && status == OrderStatus.Ready;

        if (!validTransition)
        {
            const string message =
                "The requested kitchen status transition is invalid.";

            return ApiResponse.Failure<object?>(
                StatusCodes.Status409Conflict,
                message,
                [
                    new ErrorResponse(
                        "invalid_order_status_transition",
                        message)
                ]);
        }

        order.Status = status;
        order.UpdateAt = DateTime.UtcNow;
        Repository.Update(order);

        return await SaveOrderChangeAsync(
            "Kitchen order status updated successfully.",
            cancellationToken);
    }

    public async Task<ApiResponse<object?>> ProcessPaymentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await FindActiveOrderAsync(id, cancellationToken);

        if (order is null)
        {
            return ApiResponse.NotFound<object?>("Order");
        }

        if (order.Status != OrderStatus.Pending)
        {
            const string message =
                "Only a pending order can be paid.";

            return ApiResponse.Failure<object?>(
                StatusCodes.Status409Conflict,
                message,
                [
                    new ErrorResponse(
                        "order_not_pending",
                        message)
                ]);
        }

        order.Status = OrderStatus.Confirmed;
        order.UpdateAt = DateTime.UtcNow;
        Repository.Update(order);

        return await SaveOrderChangeAsync(
            "Payment processed successfully.",
            cancellationToken);
    }

    private Task<Order?> FindActiveOrderAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Repository.GetQueryable().FirstOrDefaultAsync(
            order => order.ID == id && !order.IsDeleted,
            cancellationToken);
    }

    private async Task<ApiResponse<object?>> SaveOrderChangeAsync(
        string successMessage,
        CancellationToken cancellationToken)
    {
        var saveCount =
            await UnitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? ApiResponse.Success(successMessage)
            : ApiResponse.PersistenceFailure<object?>();
    }
}
