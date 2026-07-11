using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<ApiResponse> CreateAsync(RestaurantCreatDTO dto);
        Task<ApiResponse> GetAllAsync(ViewType type);
        Task<ApiResponse> RemoveAsync(Guid id);
        Task<ApiResponse> UpdateAsync(Guid id, RestaurantUpdateDTO dto);
    }
}
