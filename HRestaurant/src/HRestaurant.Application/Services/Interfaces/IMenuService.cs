using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IMenuService
{
    Task<ApiResponse<Guid>> CreateAsync(MenuCreateDTO dto, CancellationToken cancellationToken = default);
    Task<PagedResponse<MenuGetDTO>> GetAllAsync(MenuListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<MenuGetDTO>> GetByRestaurantAsync(Guid restaurantId, MenuListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<MenuGetDTO>> GetByCategoryAsync(Guid categoryId, MenuListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MenuGetDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, MenuUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SetAvailabilityAsync(Guid id, bool isAvailable, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SetPopularAsync(Guid id, bool isPopular, CancellationToken cancellationToken = default);
}
