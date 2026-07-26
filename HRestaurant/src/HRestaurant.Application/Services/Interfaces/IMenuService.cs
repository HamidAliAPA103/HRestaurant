using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IMenuService
    {
        Task<ApiResponse> CreateAsync(MenuCreateDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetAllAsync(ViewType type, CancellationToken cancellationToken = default);
        Task<ApiResponse> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> UpdateAsync(Guid id, MenuUpdateDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> ToggleAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetByID(Guid id, CancellationToken cancellationToken = default);
    }
}
