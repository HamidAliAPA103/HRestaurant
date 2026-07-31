using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;

namespace HRestaurant.Services.Interfaces;

public interface ITableService
{
    Task<ApiResponse<Guid>> CreateAsync(TableCreateDTO dto, CancellationToken cancellationToken = default);
    Task<PagedResponse<TableGetDTO>> GetAllAsync(TableListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TableGetDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, TableUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateStatusAsync(Guid id, TableStatusUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdatePositionAsync(Guid id, TablePositionUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateRotationAsync(Guid id, TableRotationUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateSizeAsync(Guid id, TableSizeUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SaveLayoutAsync(TableLayoutBulkUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<PublicTableLayoutDTO>>> GetPublicLayoutAsync(Guid branchId, CancellationToken cancellationToken = default);
}
