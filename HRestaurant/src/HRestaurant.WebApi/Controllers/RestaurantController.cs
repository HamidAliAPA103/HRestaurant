using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public RestaurantController(IRestaurantService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            RestaurantCreatDTO dto,
            CancellationToken cancellationToken)
        {
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
            RestaurantUpdateDTO dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(
                id,
                dto,
                cancellationToken);

            return StatusCode(result.StatusCode, result);

        }
    }
}
