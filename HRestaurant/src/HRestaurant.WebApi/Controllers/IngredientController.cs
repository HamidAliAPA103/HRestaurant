using HRestaurant.DTOS.Ingredient;
using HRestaurant.DTOS.Menu;
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
[Route("api/Ingredient")]
[Route("api/ingredients")]
public sealed class IngredientController : ApiControllerBase
{
    private readonly IIngredientService _service;

    public IngredientController(IIngredientService service) => _service = service;

    [HttpPost]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(IngredientCreateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<IngredientGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] IngredientListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IngredientGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id, IngredientUpdateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpPost("menu-items/{menuItemId:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddToMenuItem(
        Guid menuItemId, MenuItemIngredientDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.AddToMenuItemAsync(menuItemId, dto, cancellationToken));

    [HttpPut("menu-items/{menuItemId:guid}/{ingredientId:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateQuantity(
        Guid menuItemId, Guid ingredientId, MenuItemIngredientQuantityDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateQuantityAsync(
            menuItemId, ingredientId, dto.RequiredQuantity, cancellationToken));

    [HttpDelete("menu-items/{menuItemId:guid}/{ingredientId:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveFromMenuItem(
        Guid menuItemId, Guid ingredientId, CancellationToken cancellationToken) =>
        FromResponse(await _service.RemoveFromMenuItemAsync(
            menuItemId, ingredientId, cancellationToken));
}
