using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IMenuCategoryService
{
    Task<ApiResponse<Guid>> CreateAsync(MenuCategoryCreateDTO dto, CancellationToken cancellationToken = default);
    Task<PagedResponse<MenuCategoryGetDTO>> GetAllAsync(MenuCategoryListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MenuCategoryGetDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, MenuCategoryUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateDisplayOrderAsync(Guid id, MenuCategoryDisplayOrderDTO dto, CancellationToken cancellationToken = default);
}
