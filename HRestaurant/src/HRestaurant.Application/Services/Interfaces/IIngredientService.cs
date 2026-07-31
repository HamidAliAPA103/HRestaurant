using HRestaurant.DTOS.Ingredient;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IIngredientService
{
    Task<ApiResponse<Guid>> CreateAsync(IngredientCreateDTO dto, CancellationToken cancellationToken = default);
    Task<PagedResponse<IngredientGetDTO>> GetAllAsync(IngredientListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IngredientGetDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, IngredientUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> AddToMenuItemAsync(Guid menuItemId, MenuItemIngredientDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateQuantityAsync(Guid menuItemId, Guid ingredientId, decimal quantity, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> RemoveFromMenuItemAsync(Guid menuItemId, Guid ingredientId, CancellationToken cancellationToken = default);
}
