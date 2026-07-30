using HRestaurant.DTOS.Public;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IPublicReservationService
{
    Task<ApiResponse<PublicReservationCreatedDto>> CreateAsync(
        PublicCreateReservationDto dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PublicReservationDetailsDto>> LookupAsync(
        PublicReservationLookupRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PublicReservationDetailsDto>> TrackAsync(
        string trackingToken,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> CancelAsync(
        string confirmationCode,
        PublicCancelReservationDto dto,
        CancellationToken cancellationToken = default);
}
