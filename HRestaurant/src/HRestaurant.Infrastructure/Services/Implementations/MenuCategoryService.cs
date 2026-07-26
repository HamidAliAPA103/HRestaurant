using AutoMapper;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class MenuCategoryService : IMenuCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MenuCategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateAsync(
        MenuCategoryCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var category = _mapper.Map<MenuCategory>(dto);

        await _unitOfWork.Categories.AddAsync(
            category,
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
        var query = _unitOfWork.Categories.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var categories = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<MenuCategoryGetDTO>>(categories);

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
        var category = await _unitOfWork.Categories
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (category is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "MenuCategory not found!"
            };
        }

        return new ApiResponse
        {
            StatusCode = 200,
            Data = _mapper.Map<MenuCategoryGetDTO>(category)
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(
            id,
            cancellationToken);

        if (category is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "MenuCategory not found!"
            };
        }

        _unitOfWork.Categories.Delete(category);
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
        var category = await _unitOfWork.Categories.GetByIdAsync(
            id,
            cancellationToken);

        if (category is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "MenuCategory not found!"
            };
        }

        category.IsDeleted = !category.IsDeleted;
        category.DeletedAt = category.IsDeleted ? DateTime.UtcNow : null;
        category.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? category.IsDeleted
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
        MenuCategoryUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (category is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "MenuCategory not found!"
            };
        }

        _mapper.Map(dto, category);
        category.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);
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
