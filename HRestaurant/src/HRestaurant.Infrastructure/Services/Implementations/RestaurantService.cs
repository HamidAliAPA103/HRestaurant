using AutoMapper;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class RestaurantService : IRestaurantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RestaurantService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateAsync(
        RestaurantCreatDTO dto,
        CancellationToken cancellationToken = default)
    {
        var restaurant = _mapper.Map<Restaurant>(dto);

        await _unitOfWork.Restaurants.AddAsync(
            restaurant,
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
        var query = _unitOfWork.Restaurants.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var restaurants = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<RestaurantGetDTO>>(restaurants);

        return new ApiResponse
        {
            StatusCode = 200,
            Data = dtos,
            Message = $"Total: {dtos.Count}"
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(
            id,
            cancellationToken);

        if (restaurant is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Restaurant not found!"
            };
        }

        _unitOfWork.Restaurants.Delete(restaurant);
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

    public async Task<ApiResponse> UpdateAsync(
        Guid id,
        RestaurantUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await _unitOfWork.Restaurants
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (restaurant is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Restaurant not found!"
            };
        }

        _mapper.Map(dto, restaurant);
        restaurant.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Restaurants.Update(restaurant);
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
