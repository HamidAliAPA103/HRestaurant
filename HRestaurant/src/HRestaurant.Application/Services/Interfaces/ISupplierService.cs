using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Supplier;

namespace HRestaurant.Services.Interfaces;

public interface ISupplierService
{
    Task<ApiResponse<Guid>> CreateAsync(SupplierCreateDTO dto, CancellationToken cancellationToken = default);
    Task<PagedResponse<SupplierGetDTO>> GetAllAsync(SupplierListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<SupplierGetDTO>> GetByRestaurantAsync(Guid restaurantId, SupplierListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SupplierGetDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, SupplierUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
