using HRestaurant.DTOS.Common;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager)]
[Route("api/uploads")]
[Produces("application/json")]
public sealed class UploadController : ApiControllerBase
{
    private readonly IImageUploadService _service;
    public UploadController(IImageUploadService service) => _service = service;

    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImageUploadResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromForm] string category,
        [FromForm] Guid? restaurantId,
        [FromForm] string? oldImageUrl,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return FromResponse(await _service.UploadAsync(
            new FileUploadDTO
            {
                Content = stream,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length
            },
            category,
            restaurantId,
            oldImageUrl,
            cancellationToken));
    }

    [HttpDelete("images")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteImage(
        [FromQuery] string imageUrl,
        [FromQuery] Guid? restaurantId,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.DeleteAsync(imageUrl, restaurantId, cancellationToken));
}
