using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.User;
using HRestaurant.Enum;
using HRestaurant.Services.Interfaces;
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
    public async Task<IActionResult> Create(
        UserCreateDTO dto,
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
        UserUpdateDTO dto,
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
