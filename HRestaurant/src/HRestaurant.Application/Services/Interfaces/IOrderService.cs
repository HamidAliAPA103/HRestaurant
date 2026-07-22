using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse> CreateAsync(OrderCreatDTO dto);
        Task<ApiResponse> GetAllAsync(ViewType type);
        Task<ApiResponse> RemoveAsync(Guid id);
        Task<ApiResponse> UpdateAsync(Guid id, OrderUpdateDTO dto);
        Task<ApiResponse> ToggleAsync(Guid id);
        Task<ApiResponse> GetByID(Guid id);
    }
}
