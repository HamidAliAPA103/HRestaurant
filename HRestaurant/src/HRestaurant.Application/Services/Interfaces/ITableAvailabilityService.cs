using HRestaurant.DTOS.Public;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface ITableAvailabilityService
{
    Task<ApiResponse<IReadOnlyCollection<PublicRestaurantTableDto>>>
        GetTablesAsync(
            Guid branchId,
            TableAvailabilityRequestDto request,
            CancellationToken cancellationToken = default);

    Task<TableAvailabilityCheckResult> CheckAsync(
        Guid branchId,
        Guid tableId,
        DateTime startUtc,
        DateTime endUtc,
        int guestCount,
        CancellationToken cancellationToken = default);
}

public sealed record TableAvailabilityCheckResult(
    bool IsAvailable,
    string? UnavailableReason);
