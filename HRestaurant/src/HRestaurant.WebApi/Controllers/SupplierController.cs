using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Supplier;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager)]
[PermissionAuthorize(Permissions.Suppliers.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[Route("api/suppliers")]
public sealed class SupplierController : ApiControllerBase
{
    private const string OwnerRoles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner;
    private readonly ISupplierService _service;
    public SupplierController(ISupplierService service) => _service = service;

    [HttpPost, Authorize(Roles = OwnerRoles), PermissionAuthorize(Permissions.Suppliers.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(SupplierCreateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<SupplierGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SupplierListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("restaurant/{restaurantId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<SupplierGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRestaurant(Guid restaurantId,
        [FromQuery] SupplierListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByRestaurantAsync(restaurantId, request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupplierGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}"), Authorize(Roles = OwnerRoles), PermissionAuthorize(Permissions.Suppliers.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, SupplierUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}"), Authorize(Roles = OwnerRoles), PermissionAuthorize(Permissions.Suppliers.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/activate"), Authorize(Roles = OwnerRoles), PermissionAuthorize(Permissions.Suppliers.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.ActivateAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/deactivate"), Authorize(Roles = OwnerRoles), PermissionAuthorize(Permissions.Suppliers.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.DeactivateAsync(id, cancellationToken));
}
