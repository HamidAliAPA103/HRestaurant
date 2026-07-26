using AutoMapper;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class OrderItemService : IOrderItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderItemService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateAsync(
        OrderItemCreatDTO dto,
        CancellationToken cancellationToken = default)
    {
        var orderItem = _mapper.Map<OrderItem>(dto);

        await _unitOfWork.OrderItems.AddAsync(
            orderItem,
            cancellationToken);

        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? new ApiResponse
            {
                StatusCode = 201,
                Message = "Created successfully!"
            }
            : new ApiResponse
            {
                StatusCode = 500,
                Message = "Save failed!"
            };
    }

    public async Task<ApiResponse> GetAllAsync(
        ViewType type,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.OrderItems.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var orderItems = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<OrderItemGetDTO>>(orderItems);

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
        var orderItem = await _unitOfWork.OrderItems
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (orderItem is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "OrderItem not found!"
            };
        }

        return new ApiResponse
        {
            StatusCode = 200,
            Data = _mapper.Map<OrderItemGetDTO>(orderItem)
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(
            id,
            cancellationToken);

        if (orderItem is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "OrderItem not found!"
            };
        }

        _unitOfWork.OrderItems.Delete(orderItem);
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
        var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(
            id,
            cancellationToken);

        if (orderItem is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "OrderItem not found!"
            };
        }

        orderItem.IsDeleted = !orderItem.IsDeleted;
        orderItem.DeletedAt = orderItem.IsDeleted ? DateTime.UtcNow : null;
        orderItem.UpdateAt = DateTime.UtcNow;

        _unitOfWork.OrderItems.Update(orderItem);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? orderItem.IsDeleted
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
        OrderItemUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var orderItem = await _unitOfWork.OrderItems
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (orderItem is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "OrderItem not found!"
            };
        }

        _mapper.Map(dto, orderItem);
        orderItem.UpdateAt = DateTime.UtcNow;

        _unitOfWork.OrderItems.Update(orderItem);
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
