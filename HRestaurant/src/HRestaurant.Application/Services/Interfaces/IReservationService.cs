using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ApiResponse> CreateAsync(ReservationCreateDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetAllAsync(ViewType type, CancellationToken cancellationToken = default);
        Task<ApiResponse> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> UpdateAsync(Guid id, ReservationUpdateDTO dto, CancellationToken cancellationToken = default);
        Task<ApiResponse> ToggleAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse> GetByID(Guid id, CancellationToken cancellationToken = default);
    }
}
