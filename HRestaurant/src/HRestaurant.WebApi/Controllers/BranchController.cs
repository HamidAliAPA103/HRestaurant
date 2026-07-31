using HRestaurant.DTOS.Branch;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(
    Roles = AppRoles.SuperAdmin
        + "," + AppRoles.RestaurantOwner
        + "," + AppRoles.Manager)]
[PermissionAuthorize(Permissions.Branches.Read)]
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
public sealed class BranchController : ApiControllerBase
{
    private const string OwnerRoles =
        AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner;

    private readonly IBranchService _service;

    public BranchController(IBranchService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = OwnerRoles)]
    [PermissionAuthorize(Permissions.Branches.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<Guid>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        BranchCreateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<BranchGetDTO>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] BranchListRequest request,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetAllAsync(request, cancellationToken));
    }

    [HttpGet("restaurant/{restaurantId:guid}")]
    [ProducesResponseType(
        typeof(PagedResponse<BranchGetDTO>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRestaurant(
        Guid restaurantId,
        [FromQuery] BranchListRequest request,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetByRestaurantAsync(
                restaurantId,
                request,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<BranchGetDTO>),
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

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(Permissions.Branches.Manage)]
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
        BranchUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = OwnerRoles)]
    [PermissionAuthorize(Permissions.Branches.Manage)]
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
    [PermissionAuthorize(Permissions.Branches.Manage)]
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
    [PermissionAuthorize(Permissions.Branches.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.DeactivateAsync(id, cancellationToken));
    }

    [HttpPut("{id:guid}/manager")]
    [Authorize(Roles = OwnerRoles)]
    [PermissionAuthorize(Permissions.Branches.Manage)]
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
    public async Task<IActionResult> AssignManager(
        Guid id,
        BranchManagerAssignmentDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.AssignManagerAsync(
                id,
                dto,
                cancellationToken));
    }

    [HttpDelete("{id:guid}/manager")]
    [Authorize(Roles = OwnerRoles)]
    [PermissionAuthorize(Permissions.Branches.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveManager(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.RemoveManagerAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/working-hours")]
    [ProducesResponseType(
        typeof(ApiResponse<
            IReadOnlyCollection<BranchWorkingHourDTO>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkingHours(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetWorkingHoursAsync(id, cancellationToken));
    }

    [HttpPut("{id:guid}/working-hours")]
    [PermissionAuthorize(Permissions.Branches.Manage)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWorkingHours(
        Guid id,
        BranchWorkingHoursUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateWorkingHoursAsync(
                id,
                dto,
                cancellationToken));
    }
}
