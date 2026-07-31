using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.User;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager
    + "," + AppRoles.Cashier + "," + AppRoles.Waiter)]
[PermissionAuthorize(Permissions.Employees.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[Route("api/Employee")]
[Route("api/User")]
public sealed class UserController : ApiControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service) => _service = service;

    [HttpPost]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(UserCreateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] EmployeeListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("restaurant/{restaurantId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<UserGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRestaurant(
        Guid restaurantId, [FromQuery] EmployeeListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByRestaurantAsync(restaurantId, request, cancellationToken));

    [HttpGet("branch/{branchId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<UserGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBranch(
        Guid branchId, [FromQuery] EmployeeListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByBranchAsync(branchId, request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserGetDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id, UserUpdateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/activate")]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.ActivateAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/deactivate")]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.DeactivateAsync(id, cancellationToken));

    [HttpPut("{id:guid}/branch")]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignBranch(
        Guid id, EmployeeBranchAssignmentDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.AssignBranchAsync(id, dto, cancellationToken));

    [HttpPut("{id:guid}/role")]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRole(
        Guid id, EmployeeRoleAssignmentDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.AssignRoleAsync(id, dto, cancellationToken));
}
