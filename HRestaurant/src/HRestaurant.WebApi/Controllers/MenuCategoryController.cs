using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager)]
[PermissionAuthorize(Permissions.Menus.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[Route("api/Category")]
[Route("api/MenuCategory")]
public sealed class MenuCategoryController : ApiControllerBase
{
    private readonly IMenuCategoryService _service;

    public MenuCategoryController(IMenuCategoryService service) => _service = service;

    [HttpPost]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(MenuCategoryCreateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MenuCategoryGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] MenuCategoryListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("restaurant/{restaurantId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<MenuCategoryGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRestaurant(
        Guid restaurantId, [FromQuery] MenuCategoryListRequest request,
        CancellationToken cancellationToken)
    {
        request.RestaurantId = restaurantId;
        return FromResponse(await _service.GetAllAsync(request, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MenuCategoryGetDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id, MenuCategoryUpdateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/activate")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.ActivateAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/deactivate")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.DeactivateAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/display-order")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDisplayOrder(
        Guid id, MenuCategoryDisplayOrderDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateDisplayOrderAsync(id, dto, cancellationToken));
}
