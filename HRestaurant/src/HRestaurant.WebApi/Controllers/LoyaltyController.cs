using HRestaurant.DTOS.Loyalty;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
    + AppRoles.Manager + "," + AppRoles.Cashier + "," + AppRoles.Waiter)]
[PermissionAuthorize(Permissions.Loyalty.Read)]
[Route("api/customers/{customerId:guid}/loyalty")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public sealed class LoyaltyController : ApiControllerBase
{
    private readonly ILoyaltyService _service;
    public LoyaltyController(ILoyaltyService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<LoyaltySummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(Guid customerId,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetSummaryAsync(customerId, cancellationToken));

    [HttpGet("history")]
    [ProducesResponseType(typeof(PagedResponse<LoyaltyTransactionGetDTO>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid customerId,
        [FromQuery] LoyaltyHistoryRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetHistoryAsync(customerId, request, cancellationToken));

    [HttpPost("adjust"), Authorize(Roles = AppRoles.SuperAdmin + ","
        + AppRoles.RestaurantOwner + "," + AppRoles.Manager),
     PermissionAuthorize(Permissions.Loyalty.Manage)]
    [ProducesResponseType(typeof(ApiResponse<LoyaltySummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Adjust(Guid customerId, LoyaltyAdjustmentDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.AdjustAsync(customerId, dto, cancellationToken));
}
