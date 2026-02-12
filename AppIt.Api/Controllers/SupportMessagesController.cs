using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/support/messages")]
    public class SupportMessagesController : ControllerBase
    {
        private readonly ISupportMessageService _service;

        public SupportMessagesController(ISupportMessageService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? email)
        {
            var messages = await _service.GetAllAsync();
            if (string.IsNullOrWhiteSpace(email))
            {
                return Ok(messages);
            }

            var filtered = messages.Where(m =>
                string.Equals(m.FromEmail, email, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.ToEmail, email, StringComparison.OrdinalIgnoreCase));

            return Ok(filtered);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var message = await _service.GetByIdAsync(id);
            return message == null ? NotFound() : Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupportMessageDto dto)
        {
            var message = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupportMessageDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            var message = await _service.UpdateAsync(dto);
            return message == null ? NotFound() : Ok(message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
