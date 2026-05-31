using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/support/messages")]
    public class SupportMessagesController : ControllerBase
    {
        private readonly ISupportMessageService _service;

        public SupportMessagesController(ISupportMessageService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var messages = await _service.GetAllAsync();

            return Ok(messages.ApplyQuery(query,
                nameof(SupportMessageReadDto.FromEmail),
                nameof(SupportMessageReadDto.ToEmail),
                nameof(SupportMessageReadDto.Subject),
                nameof(SupportMessageReadDto.Status)));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine([FromQuery] ListQueryOptions query)
        {
            var email = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var messages = await _service.GetByEmailAsync(email);
            return Ok(messages.ApplyQuery(query,
                nameof(SupportMessageReadDto.FromEmail),
                nameof(SupportMessageReadDto.ToEmail),
                nameof(SupportMessageReadDto.Subject),
                nameof(SupportMessageReadDto.Status)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var message = await _service.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupportMessageDto dto)
        {
            var message = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupportMessageDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var message = await _service.UpdateAsync(dto);
            return message == null ? NotFound() : Ok(message);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
