using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<ApiResponse> CreateAsync(RestaurantCreatDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetAllAsync(ViewType type, CancellationToken cancellationToken = default);
        Task<ApiResponse> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> UpdateAsync(Guid id, RestaurantUpdateDTO dto, CancellationToken cancellationToken = default);
    }
}
