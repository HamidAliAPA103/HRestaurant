using HRestaurant.DTOS.Inventory;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner
    + "," + AppRoles.Manager + "," + AppRoles.Chef)]
[PermissionAuthorize(Permissions.Notifications.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[Route("api/inventory/notifications")]
public sealed class InventoryNotificationController : ApiControllerBase
{
    private const string ManageRoles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager;
    private readonly IInventoryNotificationService _service;
    public InventoryNotificationController(IInventoryNotificationService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<InventoryNotificationGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] InventoryNotificationListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("unread")]
    [ProducesResponseType(typeof(PagedResponse<InventoryNotificationGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnread([FromQuery] InventoryNotificationListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetUnreadAsync(request, cancellationToken));

    [HttpGet("unread/count")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount([FromQuery] Guid? branchId,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetUnreadCountAsync(branchId, cancellationToken));

    [HttpPatch("{id:guid}/read"), Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Notifications.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.MarkAsReadAsync(id, cancellationToken));

    [HttpPatch("read-all"), Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Notifications.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] Guid? branchId,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.MarkAllAsReadAsync(branchId, cancellationToken));

    [HttpPatch("{id:guid}/resolve"), Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Notifications.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.ResolveAsync(id, cancellationToken));
}
