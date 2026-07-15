using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Review;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ApiResponse> CreateAsync(ReviewCreateDTO dto);
        Task<ApiResponse> GetAllAsync(ViewType type);
        Task<ApiResponse> RemoveAsync(Guid id);
        Task<ApiResponse> UpdateAsync(Guid id, ReviewUpdateDTO dto);
        Task<ApiResponse> ToggleAsync(Guid id);
        Task<ApiResponse> GetByID(Guid id);
    }
}
