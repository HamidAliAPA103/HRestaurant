using AutoMapper;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateAsync(
        OrderCreatDTO dto,
        CancellationToken cancellationToken = default)
    {
        var order = _mapper.Map<Order>(dto);
        order.TotalPrices = 0;

        for (var index = 0; index < dto.Items.Count; index++)
        {
            var itemDto = dto.Items[index];
            var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(
                itemDto.MenuId,
                cancellationToken);

            if (menuItem is null || menuItem.IsDeleted)
            {
                return new ApiResponse
                {
                    StatusCode = 404,
                    Message = "Yemək tapılmadı!"
                };
            }

            var orderItem = order.Items[index];
            orderItem.OrderId = Guid.Empty;
            orderItem.Prices = menuItem.Price;
            order.TotalPrices += orderItem.Prices * orderItem.Say;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        int saveCount;

        try
        {
            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (saveCount <= 0)
            {
                await _unitOfWork.RollbackTransactionAsync(
                    CancellationToken.None);

                return new ApiResponse
                {
                    StatusCode = 500,
                    Message = "Save failed!"
                };
            }
        }
        catch (Exception operationException)
        {
            try
            {
                await _unitOfWork.RollbackTransactionAsync(
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

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return new ApiResponse
        {
            StatusCode = 201,
            Message = "Created successfully!"
        };
    }

    public async Task<ApiResponse> GetAllAsync(
        ViewType type,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Orders.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var orders = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<OrderGetDTO>>(orders);

        return new ApiResponse
        {
            StatusCode = 200,
            Data = dtos,
            Message = $"Total: {dtos.Count}"
        };
    }

    public async Task<ApiResponse> GetByID(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (order is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Order not found!"
            };
        }

        return new ApiResponse
        {
            StatusCode = 200,
            Data = _mapper.Map<OrderGetDTO>(order)
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(
            id,
            cancellationToken);

        if (order is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Order not found!"
            };
        }

        _unitOfWork.Orders.Delete(order);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? new ApiResponse
            {
                StatusCode = 204,
                Message = "Deleted successfully!"
            }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

    public async Task<ApiResponse> ToggleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(
            id,
            cancellationToken);

        if (order is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Order not found!"
            };
        }

        order.IsDeleted = !order.IsDeleted;
        order.DeletedAt = order.IsDeleted ? DateTime.UtcNow : null;
        order.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Orders.Update(order);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? order.IsDeleted
                ? new ApiResponse
                {
                    StatusCode = 204,
                    Message = "Deleted temporarily!"
                }
                : new ApiResponse
                {
                    StatusCode = 200,
                    Message = "Restored successfully!"
                }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

    public async Task<ApiResponse> UpdateAsync(
        Guid id,
        OrderUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (order is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Order not found!"
            };
        }

        _mapper.Map(dto, order);
        order.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Orders.Update(order);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? new ApiResponse
            {
                StatusCode = 200,
                Message = "Updated successfully!"
            }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

}
