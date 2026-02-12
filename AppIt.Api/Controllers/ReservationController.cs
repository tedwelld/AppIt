using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationController(IReservationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _service.GetAllAsync();
            return Ok(reservations);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine([FromQuery] string? email)
        {
            var reservations = await _service.GetAllAsync();
            if (string.IsNullOrWhiteSpace(email))
            {
                return Ok(reservations);
            }

            var filtered = reservations.Where(r => string.Equals(r.CustomerEmail, email, StringComparison.OrdinalIgnoreCase));
            return Ok(filtered);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _service.GetByIdAsync(id);
            if (reservation == null) return NotFound();
            return Ok(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var reservation = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = reservation.ReservationId }, reservation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            if (id != dto.ReservationId) return BadRequest("ID mismatch");

            var reservation = await _service.UpdateAsync(dto);
            if (reservation == null) return NotFound();
            return Ok(reservation);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (result.NotFound) return NotFound();
            if (!result.Success) return BadRequest(result);
            return NoContent();
        }
    }
}
