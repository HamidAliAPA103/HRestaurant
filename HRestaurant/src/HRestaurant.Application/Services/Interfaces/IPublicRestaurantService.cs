using HRestaurant.DTOS.Public;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IPublicRestaurantService
{
    Task<ApiResponse<IReadOnlyCollection<PublicRestaurantDto>>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PublicRestaurantDto>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<PublicBranchDto>>> GetBranchesAsync(
        string restaurantSlug,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<PublicMenuCategoryDto>>> GetMenuAsync(
        string restaurantSlug,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PublicMenuItem3DDto>> GetMenuItem3DAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<PublicIngredient3DDto>>>
        GetMenuItemIngredients3DAsync(
            Guid menuItemId,
            CancellationToken cancellationToken = default);
}
