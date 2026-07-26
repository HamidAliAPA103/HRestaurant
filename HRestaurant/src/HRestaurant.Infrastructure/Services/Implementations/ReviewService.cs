using AutoMapper;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Review;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReviewService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateAsync(
        ReviewCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var review = _mapper.Map<Review>(dto);

        await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
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
        var query = _unitOfWork.Reviews.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var reviews = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<ReviewGetDTO>>(reviews);

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
        var review = await _unitOfWork.Reviews
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (review is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Review not found!"
            };
        }

        return new ApiResponse
        {
            StatusCode = 200,
            Data = _mapper.Map<ReviewGetDTO>(review)
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(
            id,
            cancellationToken);

        if (review is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Review not found!"
            };
        }

        _unitOfWork.Reviews.Delete(review);
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
        var review = await _unitOfWork.Reviews.GetByIdAsync(
            id,
            cancellationToken);

        if (review is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Review not found!"
            };
        }

        review.IsDeleted = !review.IsDeleted;
        review.DeletedAt = review.IsDeleted ? DateTime.UtcNow : null;
        review.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? review.IsDeleted
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
        ReviewUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (review is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "Review not found!"
            };
        }

        _mapper.Map(dto, review);
        review.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);
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
