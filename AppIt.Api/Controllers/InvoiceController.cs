using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _service;
        private readonly ICurrentUserService _currentUser;
        private readonly IResourceAuthorizationService _resourceAuth;

        public InvoiceController(
            IInvoiceService service,
            ICurrentUserService currentUser,
            IResourceAuthorizationService resourceAuth)
        {
            _service = service;
            _currentUser = currentUser;
            _resourceAuth = resourceAuth;
        }

        [HttpGet]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var invoices = await _service.GetAllAsync();

            return Ok(invoices.ApplyQuery(query,
                nameof(InvoiceReadDto.Id),
                nameof(InvoiceReadDto.ReservationId),
                nameof(InvoiceReadDto.Status),
                nameof(InvoiceReadDto.Currency),
                nameof(InvoiceReadDto.IssuedAt)));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine([FromQuery] int? accountId, [FromQuery] ListQueryOptions query)
        {
            if (!_currentUser.IsStaff && accountId is > 0 && accountId != _currentUser.UserId)
            {
                return Forbid();
            }

            var resolvedAccountId = _currentUser.ResolveMineAccountId(accountId);
            var invoices = resolvedAccountId is > 0
                ? await _service.GetByAccountIdAsync(resolvedAccountId.Value)
                : Enumerable.Empty<InvoiceReadDto>();

            return Ok(invoices.ApplyQuery(query,
                nameof(InvoiceReadDto.Id),
                nameof(InvoiceReadDto.ReservationId),
                nameof(InvoiceReadDto.Status),
                nameof(InvoiceReadDto.Currency),
                nameof(InvoiceReadDto.IssuedAt)));
        }

        [HttpGet("reservation/{reservationId:int}")]
        [HttpGet("by-reservation/{reservationId:int}")]
        public async Task<IActionResult> GetByReservationId(int reservationId)
        {
            if (reservationId <= 0)
            {
                return BadRequest("Reservation ID is required.");
            }

            if (!await _resourceAuth.CanAccessReservationAsync(reservationId))
            {
                return Forbid();
            }

            var invoice = await _service.GetByReservationIdAsync(reservationId);
            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        [HttpGet("reservation")]
        public async Task<IActionResult> GetByReservationQuery([FromQuery] int reservationId)
        {
            return await GetByReservationId(reservationId);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await _resourceAuth.CanAccessInvoiceAsync(id))
            {
                return Forbid();
            }

            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        [HttpGet("verification")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> VerifyPayments([FromQuery] string granularity = "day", [FromQuery] DateTime? atUtc = null)
        {
            var result = await _service.VerifyPaymentsAsync(granularity, atUtc);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "super,admin")]
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
        [Authorize(Roles = "super,admin")]
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
        [Authorize(Roles = "super,admin")]
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
