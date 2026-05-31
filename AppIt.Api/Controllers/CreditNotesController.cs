using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/credit-notes")]
    public class CreditNotesController : ControllerBase
    {
        private readonly ICreditNoteService _service;

        public CreditNotesController(ICreditNoteService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var items = await _service.GetAllAsync();
            return Ok(items.ApplyQuery(query,
                nameof(CreditNoteReadDto.Status),
                nameof(CreditNoteReadDto.CurrencyCode),
                nameof(CreditNoteReadDto.Reason)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("by-reservation/{reservationId:int}")]
        public async Task<IActionResult> GetByReservation(int reservationId)
            => Ok(await _service.GetByReservationAsync(reservationId));

        [HttpGet("by-invoice/{invoiceId:int}")]
        public async Task<IActionResult> GetByInvoice(int invoiceId)
            => Ok(await _service.GetByInvoiceAsync(invoiceId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCreditNoteDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCreditNoteDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            var item = await _service.UpdateAsync(dto);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
