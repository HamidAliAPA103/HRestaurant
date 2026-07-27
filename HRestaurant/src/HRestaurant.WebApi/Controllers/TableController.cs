using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;
using HRestaurant.Enum;
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
    public async Task<IActionResult> Create(
        TableCreateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> Remove(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.RemoveAsync(id, cancellationToken));
    }

    [HttpGet]
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
    public async Task<IActionResult> Update(
        Guid id,
        TableUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpPatch("toggle/{id:guid}")]
    public async Task<IActionResult> Toggle(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.ToggleAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetByIdAsync(id, cancellationToken));
    }
}
