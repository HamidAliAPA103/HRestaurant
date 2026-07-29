using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.Enum;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Route("api/[controller]")]
public sealed class TableController : ApiControllerBase
{
    private readonly ITableService _service;

    public TableController(ITableService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Tables.Manage)]
    public async Task<IActionResult> Create(
        TableCreateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpDelete]
    [PermissionAuthorize(Permissions.Tables.Manage)]
    public async Task<IActionResult> Remove(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.RemoveAsync(id, cancellationToken));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Tables.Read)]
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
    [PermissionAuthorize(Permissions.Tables.Manage)]
    public async Task<IActionResult> Update(
        Guid id,
        TableUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpPatch("toggle/{id:guid}")]
    [PermissionAuthorize(Permissions.Tables.Manage)]
    public async Task<IActionResult> Toggle(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.ToggleAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize(Permissions.Tables.Read)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetByIdAsync(id, cancellationToken));
    }
}
