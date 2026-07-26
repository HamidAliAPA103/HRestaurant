using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IOrderItemService
    {
        Task<ApiResponse> CreateAsync(OrderItemCreatDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetAllAsync(ViewType type, CancellationToken cancellationToken = default);
        Task<ApiResponse> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> UpdateAsync(Guid id, OrderItemUpdateDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> ToggleAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetByID(Guid id, CancellationToken cancellationToken = default);
    }
}
