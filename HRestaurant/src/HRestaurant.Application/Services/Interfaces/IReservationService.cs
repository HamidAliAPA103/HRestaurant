using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IReservationService :
    ICrudService<
        ReservationCreateDTO,
        ReservationUpdateDTO,
        ReservationGetDTO>
{
    Task<PagedResponse<ReservationGetDTO>> GetAllAsync(
        ReservationListRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> UpdateStatusAsync(
        Guid id,
        ReservationStatusUpdateDTO dto,
        CancellationToken cancellationToken = default);
}
