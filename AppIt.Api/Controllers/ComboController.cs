using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppIt.Core.Interfaces.Services;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/combos")]
    public class ComboController : ControllerBase
    {
        private readonly IComboService _service;

        public ComboController(IComboService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppIt.Core.DTOs.CreateComboDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppIt.Core.DTOs.UpdateComboDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) =>
            await _service.DeleteAsync(id) ? NoContent() : NotFound();

        [HttpGet("{id}/splits")]
        public async Task<IActionResult> PreviewSplits(int id, [FromQuery] decimal total = 100, [FromQuery] int quantity = 1) =>
            Ok(await _service.ExpandComboSplitsAsync(id, total, quantity));
    }
}
