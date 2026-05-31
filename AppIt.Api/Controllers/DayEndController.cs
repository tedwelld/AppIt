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
    [Authorize(Roles = "super,admin")]
    [Route("api/day-end")]
    public class DayEndController : ControllerBase
    {
        private readonly IDayEndService _service;

        public DayEndController(IDayEndService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var items = await _service.GetAllAsync();
            return Ok(items.ApplyQuery(query,
                nameof(DayEndReadDto.Status),
                nameof(DayEndReadDto.OpenedBy),
                nameof(DayEndReadDto.ClosedBy)));
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var item = await _service.GetTodayAsync();
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost("open")]
        public async Task<IActionResult> Open([FromBody] OpenDayEndDto dto)
        {
            var openedBy = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? User.Identity?.Name ?? "Unknown";

            var item = await _service.OpenAsync(dto, openedBy);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPost("close")]
        public async Task<IActionResult> Close([FromBody] CloseDayEndDto dto)
        {
            var closedBy = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? User.Identity?.Name ?? "Unknown";

            var item = await _service.CloseAsync(dto, closedBy);
            return item == null ? NotFound() : Ok(item);
        }
    }
}
