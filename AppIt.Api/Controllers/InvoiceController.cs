using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _service;

        public InvoiceController(IInvoiceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var invoices = await _service.GetAllAsync();

            return Ok(invoices.ApplyQuery(query,
                nameof(InvoiceReadDto.ReservationId),
                nameof(InvoiceReadDto.Status),
                nameof(InvoiceReadDto.Currency)));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine([FromQuery] ListQueryOptions query)
        {
            var invoices = await _service.GetAllAsync();
            return Ok(invoices.ApplyQuery(query,
                nameof(InvoiceReadDto.ReservationId),
                nameof(InvoiceReadDto.Status),
                nameof(InvoiceReadDto.Currency)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        [HttpGet("verification")]
        public async Task<IActionResult> VerifyPayments([FromQuery] string granularity = "day", [FromQuery] DateTime? atUtc = null)
        {
            var result = await _service.VerifyPaymentsAsync(granularity, atUtc);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var invoice = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInvoiceDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var invoice = await _service.UpdateAsync(dto);
            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
