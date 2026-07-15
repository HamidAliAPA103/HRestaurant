using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface ITableService
    {
        Task<ApiResponse> CreateAsync(TableCreateDTO dto);
        Task<ApiResponse> GetAllAsync(ViewType type);
        Task<ApiResponse> RemoveAsync(Guid id);
        Task<ApiResponse> UpdateAsync(Guid id, TableUpdateDTO dto);
        Task<ApiResponse> ToggleAsync(Guid id);
        Task<ApiResponse> GetByID(Guid id);
    }
}
