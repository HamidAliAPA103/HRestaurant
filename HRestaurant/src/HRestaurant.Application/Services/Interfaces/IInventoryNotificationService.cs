using HRestaurant.DTOS.Inventory;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IInventoryNotificationService
{
    Task<PagedResponse<InventoryNotificationGetDTO>> GetAllAsync(InventoryNotificationListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryNotificationGetDTO>> GetUnreadAsync(InventoryNotificationListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> GetUnreadCountAsync(Guid? branchId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> MarkAllAsReadAsync(Guid? branchId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> ResolveAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IInventoryAlertService
{
    Task EvaluateItemAsync(Guid inventoryItemId, CancellationToken cancellationToken = default);
    Task<int> ScanAsync(CancellationToken cancellationToken = default);
}
