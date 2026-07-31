using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager
    + "," + AppRoles.Cashier + "," + AppRoles.Waiter)]
[PermissionAuthorize(Permissions.Tables.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[Route("api/tables")]
[Route("api/Table")]
public sealed class TableController : ApiControllerBase
{
    private readonly ITableService _service;
    public TableController(ITableService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TableGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TableListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TableGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPost, PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(TableCreateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpPut("{id:guid}"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, TableUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/status"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(Guid id, TableStatusUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateStatusAsync(id, dto, cancellationToken));

    [HttpPatch("{id:guid}/position"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePosition(Guid id, TablePositionUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdatePositionAsync(id, dto, cancellationToken));

    [HttpPatch("{id:guid}/rotation"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRotation(Guid id, TableRotationUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateRotationAsync(id, dto, cancellationToken));

    [HttpPatch("{id:guid}/size"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSize(Guid id, TableSizeUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateSizeAsync(id, dto, cancellationToken));

    [HttpPut("layout"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveLayout(TableLayoutBulkUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.SaveLayoutAsync(dto, cancellationToken));

    [HttpPatch("{id:guid}/activate"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.ActivateAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/deactivate"), PermissionAuthorize(Permissions.Tables.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.DeactivateAsync(id, cancellationToken));
}
