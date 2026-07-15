using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ApiResponse> CreateAsync(ReservationCreateDTO dto);
        Task<ApiResponse> GetAllAsync(ViewType type);
        Task<ApiResponse> RemoveAsync(Guid id);
        Task<ApiResponse> UpdateAsync(Guid id, ReservationUpdateDTO dto);
        Task<ApiResponse> ToggleAsync(Guid id);
        Task<ApiResponse> GetByID(Guid id);
    }
}
