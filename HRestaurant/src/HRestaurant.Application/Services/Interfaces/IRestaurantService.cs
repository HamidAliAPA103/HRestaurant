using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;

namespace HRestaurant.Services.Interfaces;

public interface IRestaurantService
{
    Task<ApiResponse<Guid>> CreateAsync(
        RestaurantCreateDTO dto,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<RestaurantGetDTO>> GetAllAsync(
        ViewType type,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<RestaurantGetDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<RestaurantGetDTO>> GetCurrentAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        RestaurantUpdateDTO dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<RestaurantWorkingHourDTO>>>
        GetWorkingHoursAsync(
            Guid id,
            CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> UpdateWorkingHoursAsync(
        Guid id,
        RestaurantWorkingHoursUpdateDTO dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> UpdateSettingsAsync(
        Guid id,
        RestaurantSettingsUpdateDTO dto,
        CancellationToken cancellationToken = default);
}
