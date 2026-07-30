using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(
    Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner)]
[PermissionAuthorize(Permissions.Restaurants.Read)]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ApiResponse<object>),
    StatusCodes.Status401Unauthorized)]
[ProducesResponseType(
    typeof(ApiResponse<object>),
    StatusCodes.Status403Forbidden)]
[ProducesResponseType(
    typeof(ApiResponse<object>),
    StatusCodes.Status500InternalServerError)]
[Route("api/[controller]")]
public sealed class RestaurantController : ApiControllerBase
{
    private readonly IRestaurantService _service;

    public RestaurantController(IRestaurantService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    [PermissionAuthorize(Permissions.Restaurants.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<Guid>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        RestaurantCreateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<RestaurantGetDTO>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        ViewType type,
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetAllAsync(
                type,
                pagination,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<RestaurantGetDTO>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("current")]
    [ProducesResponseType(
        typeof(ApiResponse<RestaurantGetDTO>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent(
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetCurrentAsync(cancellationToken));
    }

    [HttpPatch]
    [HttpPatch("{id:guid}")]
    [PermissionAuthorize(Permissions.Restaurants.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        RestaurantUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateAsync(
                id,
                dto,
                cancellationToken));
    }

    [HttpDelete]
    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Restaurants.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.SoftDeleteAsync(id, cancellationToken));
    }

    [HttpPatch("{id:guid}/activate")]
    [PermissionAuthorize(Permissions.Restaurants.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.ActivateAsync(id, cancellationToken));
    }

    [HttpPatch("{id:guid}/deactivate")]
    [PermissionAuthorize(Permissions.Restaurants.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.DeactivateAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/working-hours")]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyCollection<RestaurantWorkingHourDTO>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkingHours(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetWorkingHoursAsync(
                id,
                cancellationToken));
    }

    [HttpPut("{id:guid}/working-hours")]
    [PermissionAuthorize(Permissions.Restaurants.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateWorkingHours(
        Guid id,
        RestaurantWorkingHoursUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateWorkingHoursAsync(
                id,
                dto,
                cancellationToken));
    }

    [HttpPut("{id:guid}/settings")]
    [PermissionAuthorize(Permissions.Restaurants.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateSettings(
        Guid id,
        RestaurantSettingsUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateSettingsAsync(
                id,
                dto,
                cancellationToken));
    }
}
