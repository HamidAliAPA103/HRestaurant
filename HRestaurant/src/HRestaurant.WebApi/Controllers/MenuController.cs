using HRestaurant.DTOS.Common;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using HRestaurant.WebApi.Models.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager
    + "," + AppRoles.Cashier + "," + AppRoles.Waiter + "," + AppRoles.Chef)]
[PermissionAuthorize(Permissions.Menus.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[Route("api/MenuItem")]
[Route("api/Menu")]
public sealed class MenuController : ApiControllerBase
{
    private readonly IMenuService _service;

    public MenuController(IMenuService service) => _service = service;

    [HttpPost]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromForm] MenuCreateRequest request, CancellationToken cancellationToken)
    {
        await using var imageStream = request.Image?.OpenReadStream();
        var dto = new MenuCreateDTO
        {
            Image = request.Image is null || imageStream is null ? null : new FileUploadDTO
            {
                Content = imageStream,
                FileName = request.Image.FileName,
                ContentType = request.Image.ContentType,
                Length = request.Image.Length
            },
            ImageUrl = request.ImageUrl,
            Model3DUrl = request.Model3DUrl,
            ModelPosterUrl = request.ModelPosterUrl,
            ModelScale = request.ModelScale,
            ModelRotationX = request.ModelRotationX,
            ModelRotationY = request.ModelRotationY,
            ModelRotationZ = request.ModelRotationZ,
            Is3DEnabled = request.Is3DEnabled,
            Name = request.Name,
            Price = request.Price,
            DiscountPercentage = request.DiscountPercentage,
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            Desc = request.Desc,
            CategoryId = request.CategoryId,
            Nutrition = request.Nutrition,
            Ingredients = request.Ingredients
        };
        return FromResponse(await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MenuGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] MenuListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("restaurant/{restaurantId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<MenuGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRestaurant(
        Guid restaurantId, [FromQuery] MenuListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByRestaurantAsync(restaurantId, request, cancellationToken));

    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<MenuGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(
        Guid categoryId, [FromQuery] MenuListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByCategoryAsync(categoryId, request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MenuGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id, [FromForm] MenuUpdateRequest request, CancellationToken cancellationToken)
    {
        await using var imageStream = request.Image?.OpenReadStream();
        var dto = new MenuUpdateDTO
        {
            Image = request.Image is null || imageStream is null ? null : new FileUploadDTO
            {
                Content = imageStream,
                FileName = request.Image.FileName,
                ContentType = request.Image.ContentType,
                Length = request.Image.Length
            },
            ImageURL = request.ImageURL,
            Model3DUrl = request.Model3DUrl,
            ModelPosterUrl = request.ModelPosterUrl,
            ModelScale = request.ModelScale,
            ModelRotationX = request.ModelRotationX,
            ModelRotationY = request.ModelRotationY,
            ModelRotationZ = request.ModelRotationZ,
            Is3DEnabled = request.Is3DEnabled,
            Name = request.Name,
            Price = request.Price,
            DiscountPercentage = request.DiscountPercentage,
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            CategoryId = request.CategoryId,
            Desc = request.Desc,
            Nutrition = request.Nutrition,
            Ingredients = request.Ingredients
        };
        return FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/availability/{isAvailable:bool}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetAvailability(
        Guid id, bool isAvailable, CancellationToken cancellationToken) =>
        FromResponse(await _service.SetAvailabilityAsync(id, isAvailable, cancellationToken));

    [HttpPatch("{id:guid}/popular/{isPopular:bool}")]
    [PermissionAuthorize(Permissions.Menus.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetPopular(
        Guid id, bool isPopular, CancellationToken cancellationToken) =>
        FromResponse(await _service.SetPopularAsync(id, isPopular, cancellationToken));
}
