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
    [Route("api/vouchers")]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _service;
        private readonly ICurrentUserService _currentUser;
        private readonly IResourceAuthorizationService _resourceAuth;

        public VouchersController(
            IVoucherService service,
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
            if (!_currentUser.IsStaff && accountId is > 0 && accountId != _currentUser.UserId)
            {
                return Forbid();
            }

            var resolvedAccountId = _currentUser.ResolveMineAccountId(accountId);
            var vouchers = resolvedAccountId is > 0
                ? await _service.GetByAccountIdAsync(resolvedAccountId.Value)
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

            if (!await _resourceAuth.CanAccessReservationAsync(reservationId))
            {
                return Forbid();
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
            if (!await _resourceAuth.CanAccessVoucherAsync(id))
            {
                return Forbid();
            }

            var item = await _service.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "super,admin")]
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
