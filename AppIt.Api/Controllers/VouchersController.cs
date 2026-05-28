using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/vouchers")]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _service;

        public VouchersController(IVoucherService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var vouchers = await _service.GetAllAsync();

            return Ok(vouchers.ApplyQuery(query,
                nameof(VoucherReadDto.Code),
                nameof(VoucherReadDto.Reference),
                nameof(VoucherReadDto.Type),
                nameof(VoucherReadDto.CreatedAt)));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine([FromQuery] int? accountId, [FromQuery] ListQueryOptions query)
        {
            var vouchers = accountId.HasValue && accountId.Value > 0
                ? await _service.GetByAccountIdAsync(accountId.Value)
                : Enumerable.Empty<VoucherReadDto>();

            return Ok(vouchers.ApplyQuery(query,
                nameof(VoucherReadDto.Code),
                nameof(VoucherReadDto.Reference),
                nameof(VoucherReadDto.Type),
                nameof(VoucherReadDto.CreatedAt)));
        }

        [HttpGet("by-reservation/{reservationId:int}")]
        public async Task<IActionResult> GetByReservationId(int reservationId)
        {
            if (reservationId <= 0)
            {
                return BadRequest("Invalid reservation ID.");
            }

            var voucher = await _service.GetByReservationIdAsync(reservationId);
            if (voucher == null)
            {
                return NotFound();
            }

            return Ok(voucher);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVoucherDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var item = await _service.UpdateAsync(dto);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
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
