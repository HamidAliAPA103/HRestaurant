using HRestaurant.DTOS.Common;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Services.Interfaces;
using HRestaurant.WebApi.Models.Menu;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Route("api/[controller]")]
public sealed class MenuController : ApiControllerBase
{
    private readonly IMenuService _service;

    public MenuController(IMenuService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] MenuCreateRequest request,
        CancellationToken cancellationToken)
    {
        await using var imageStream = request.Image.OpenReadStream();

        var dto = new MenuCreateDTO
        {
            Image = new FileUploadDTO
            {
                Content = imageStream,
                FileName = request.Image.FileName,
                ContentType = request.Image.ContentType,
                Length = request.Image.Length
            },
            Price = request.Price,
            Desc = request.Desc,
            CategoryId = request.CategoryId,
            Nutrition = request.Nutrition
        };

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
        [FromForm] MenuUpdateRequest request,
        CancellationToken cancellationToken)
    {
        await using var imageStream = request.Image?.OpenReadStream();

        var dto = new MenuUpdateDTO
        {
            Image = request.Image is null || imageStream is null
                ? null
                : new FileUploadDTO
                {
                    Content = imageStream,
                    FileName = request.Image.FileName,
                    ContentType = request.Image.ContentType,
                    Length = request.Image.Length
                },
            ImageURL = request.ImageURL,
            Price = request.Price,
            Desc = request.Desc,
            Nutrition = request.Nutrition
        };

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
