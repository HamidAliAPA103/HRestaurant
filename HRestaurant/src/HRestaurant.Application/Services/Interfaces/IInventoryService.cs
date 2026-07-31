using HRestaurant.DTOS.Inventory;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IInventoryService
{
    Task<ApiResponse<Guid>> CreateAsync(InventoryItemCreateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventoryItemGetDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryItemGetDTO>> GetAllAsync(InventoryListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryItemGetDTO>> GetByRestaurantAsync(Guid restaurantId, InventoryListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryItemGetDTO>> GetByBranchAsync(Guid branchId, InventoryListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, InventoryItemUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventoryItemGetDTO>> StockInAsync(Guid id, StockMovementDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventoryItemGetDTO>> StockOutAsync(Guid id, StockMovementDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<InventoryItemGetDTO>> AdjustAsync(Guid id, StockAdjustmentDTO dto, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryItemGetDTO>> GetExpiredAsync(InventoryListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryItemGetDTO>> GetExpiringSoonAsync(InventoryListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryItemGetDTO>> GetLowStockAsync(InventoryListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<StockTransactionGetDTO>> GetTransactionsAsync(Guid inventoryItemId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
