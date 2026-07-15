using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IMenuCategoryService
    {
        Task<ApiResponse> CreateAsync(MenuCategoryCreateDTO dto);
        Task<ApiResponse> GetAllAsync(ViewType type);
        Task<ApiResponse> RemoveAsync(Guid id);
        Task<ApiResponse> UpdateAsync(Guid id, MenuCategoryUpdateDTO dto);
        Task<ApiResponse> ToggleAsync(Guid id);
        Task<ApiResponse> GetByID(Guid id);
    }
}
