using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces;

public interface ICrudService<TCreate, TUpdate, TGet>
{
    Task<ApiResponse<Guid>> CreateAsync(
        TCreate dto,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<TGet>> GetAllAsync(
        ViewType type,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<TGet>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        TUpdate dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> ToggleAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
