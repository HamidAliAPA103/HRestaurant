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
[PermissionAuthorize(Permissions.Inventory.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[Route("api/inventory")]
public sealed class InventoryController : ApiControllerBase
{
    private const string ManageRoles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager;
    private readonly IInventoryService _service;
    public InventoryController(IInventoryService service) => _service = service;

    [HttpPost, Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Inventory.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(InventoryItemCreateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] InventoryListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpGet("restaurant/{restaurantId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRestaurant(Guid restaurantId,
        [FromQuery] InventoryListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByRestaurantAsync(restaurantId, request, cancellationToken));

    [HttpGet("branch/{branchId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBranch(Guid branchId,
        [FromQuery] InventoryListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByBranchAsync(branchId, request, cancellationToken));

    [HttpPut("{id:guid}"), Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Inventory.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, InventoryItemUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}"), Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Inventory.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpPost("{id:guid}/stock-in"), Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Inventory.Manage)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StockIn(Guid id, StockMovementDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.StockInAsync(id, dto, cancellationToken));

    [HttpPost("{id:guid}/stock-out"), Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Inventory.Manage)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StockOut(Guid id, StockMovementDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.StockOutAsync(id, dto, cancellationToken));

    [HttpPost("{id:guid}/adjust"), Authorize(Roles = ManageRoles), PermissionAuthorize(Permissions.Inventory.Adjust)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Adjust(Guid id, StockAdjustmentDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.AdjustAsync(id, dto, cancellationToken));

    [HttpGet("expired")]
    [ProducesResponseType(typeof(PagedResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpired([FromQuery] InventoryListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetExpiredAsync(request, cancellationToken));

    [HttpGet("expiring-soon")]
    [ProducesResponseType(typeof(PagedResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringSoon([FromQuery] InventoryListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetExpiringSoonAsync(request, cancellationToken));

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(PagedResponse<InventoryItemGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock([FromQuery] InventoryListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetLowStockAsync(request, cancellationToken));

    [HttpGet("{id:guid}/transactions")]
    [ProducesResponseType(typeof(PagedResponse<StockTransactionGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(Guid id,
        [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetTransactionsAsync(id, pagination.PageNumber,
            pagination.PageSize, cancellationToken));
}
