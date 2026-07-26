using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Review;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ApiResponse> CreateAsync(ReviewCreateDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetAllAsync(ViewType type, CancellationToken cancellationToken = default);
        Task<ApiResponse> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> UpdateAsync(Guid id, ReviewUpdateDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> ToggleAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetByID(Guid id, CancellationToken cancellationToken = default);
    }
}
