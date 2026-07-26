using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Common;
using HRestaurant.Enum;
using HRestaurant.Services.Interfaces;
using HRestaurant.WebApi.Models.Menu;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
       private readonly IMenuService _service;

        public MenuController(IMenuService service)
        {
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

            var result = await _service.CreateAsync(dto, cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete]

        public async Task<IActionResult> Remove(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.RemoveAsync(id, cancellationToken);

            return StatusCode(result.StatusCode, result);

        }

        [HttpGet]

        public async Task<IActionResult> GetAll(
            ViewType type,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(type, cancellationToken);

            return StatusCode(result.StatusCode, result);
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

            var result = await _service.UpdateAsync(
                id,
                dto,
                cancellationToken);

            return StatusCode(result.StatusCode, result);

        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.ToggleAsync(id, cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByID(id, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
