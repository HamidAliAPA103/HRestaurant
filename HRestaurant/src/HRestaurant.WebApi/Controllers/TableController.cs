using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Table;
using HRestaurant.Enum;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableService _service;

        public TableController(ITableService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(TableCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);

            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete]

        public async Task<IActionResult> Remove(Guid id)
        {
            var result = await _service.RemoveAsync(id);

            return StatusCode(result.StatusCode, result);

        }

        [HttpGet]

        public async Task<IActionResult> GetAll(ViewType type)
        {
            var result = await _service.GetAllAsync(type);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch]
        public async Task<IActionResult> Update(Guid id, TableUpdateDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            return StatusCode(result.StatusCode, result);

        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            var result = await _service.ToggleAsync(id);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByID(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
