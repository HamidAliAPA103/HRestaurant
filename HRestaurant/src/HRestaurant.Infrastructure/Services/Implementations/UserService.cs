using AutoMapper;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.User;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(mapper);

        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse> CreateAsync(
        UserCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var user = _mapper.Map<User>(dto);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
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
        var query = _unitOfWork.Users.GetQueryable();

        query = type switch
        {
            ViewType.deleted => query.Where(entity => entity.IsDeleted),
            ViewType.notdeleted => query.Where(entity => !entity.IsDeleted),
            _ => query
        };

        var users = await query.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<UserGetDTO>>(users);

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
        var user = await _unitOfWork.Users
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (user is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "User not found!"
            };
        }

        return new ApiResponse
        {
            StatusCode = 200,
            Data = _mapper.Map<UserGetDTO>(user)
        };
    }

    public async Task<ApiResponse> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(
            id,
            cancellationToken);

        if (user is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "User not found!"
            };
        }

        _unitOfWork.Users.Delete(user);
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
        var user = await _unitOfWork.Users.GetByIdAsync(
            id,
            cancellationToken);

        if (user is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "User not found!"
            };
        }

        user.IsDeleted = !user.IsDeleted;
        user.DeletedAt = user.IsDeleted ? DateTime.UtcNow : null;
        user.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        var saveCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveCount > 0
            ? user.IsDeleted
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
        UserUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users
            .GetQueryable()
            .FirstOrDefaultAsync(
                entity => !entity.IsDeleted && entity.ID == id,
                cancellationToken);

        if (user is null)
        {
            return new ApiResponse
            {
                StatusCode = 404,
                Message = "User not found!"
            };
        }

        _mapper.Map(dto, user);
        user.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
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
