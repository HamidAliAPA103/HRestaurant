using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Restaurant;
using HRestaurant.Enum;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            OrderCreatDTO dto,
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
            OrderUpdateDTO dto,
            CancellationToken cancellationToken)
        {
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
