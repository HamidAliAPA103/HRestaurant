using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.User;
using HRestaurant.Enum;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Route("api/[controller]")]
public sealed class UserController : ApiControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    public async Task<IActionResult> Create(
        UserCreateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpDelete]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    public async Task<IActionResult> Remove(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.RemoveAsync(id, cancellationToken));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Employees.Read)]
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

    [HttpPatch]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    public async Task<IActionResult> Update(
        Guid id,
        UserUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpPatch("toggle/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    [PermissionAuthorize(Permissions.Employees.Manage)]
    public async Task<IActionResult> Toggle(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.ToggleAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize(Permissions.Employees.Read)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetByIdAsync(id, cancellationToken));
    }
}
