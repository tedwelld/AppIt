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
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly ICurrentUserService _currentUser;
        private readonly IResourceAuthorizationService _resourceAuth;

        public PaymentsController(
            IPaymentService service,
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
            var items = await _service.GetAllAsync();
            return Ok(items.ApplyQuery(query,
                nameof(PaymentReadDto.InvoiceId),
                nameof(PaymentReadDto.Method),
                nameof(PaymentReadDto.Status),
                nameof(PaymentReadDto.TransactionReference),
                nameof(PaymentReadDto.CurrencyCode),
                nameof(PaymentReadDto.ProcessedAt)));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine([FromQuery] int? accountId, [FromQuery] ListQueryOptions query)
        {
            if (!_currentUser.IsStaff && accountId is > 0 && accountId != _currentUser.UserId)
            {
                return Forbid();
            }

            var resolvedAccountId = _currentUser.ResolveMineAccountId(accountId);
            var items = resolvedAccountId is > 0
                ? await _service.GetByAccountIdAsync(resolvedAccountId.Value)
                : Enumerable.Empty<PaymentReadDto>();

            return Ok(items.ApplyQuery(query,
                nameof(PaymentReadDto.InvoiceId),
                nameof(PaymentReadDto.Method),
                nameof(PaymentReadDto.Status),
                nameof(PaymentReadDto.TransactionReference),
                nameof(PaymentReadDto.CurrencyCode),
                nameof(PaymentReadDto.ProcessedAt)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await _resourceAuth.CanAccessPaymentAsync(id))
            {
                return Forbid();
            }

            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] ProcessPaymentDto dto)
        {
            if (!await _resourceAuth.CanAccessInvoiceAsync(dto.InvoiceId))
            {
                return Forbid();
            }

            var headerKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerKey) && string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            {
                dto.IdempotencyKey = headerKey.Trim();
            }

            var result = await _service.ProcessAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var item = await _service.UpdateAsync(dto);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
