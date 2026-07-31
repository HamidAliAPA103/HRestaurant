using HRestaurant.DTOS.Customer;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<Guid>> CreateAsync(CustomerCreateDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDetailDTO>> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<CustomerGetDTO>> GetAllAsync(CustomerListRequest request,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, CustomerUpdateDTO dto,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<CustomerOrderHistoryDTO>> GetOrderHistoryAsync(Guid id,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResponse<CustomerReservationHistoryDTO>> GetReservationHistoryAsync(Guid id,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<FavoriteMenuItemDTO>>> GetFavoritesAsync(Guid id,
        CancellationToken cancellationToken = default);
}
