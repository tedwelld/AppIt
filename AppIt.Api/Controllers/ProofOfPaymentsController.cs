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
    [Route("api/proof-of-payments")]
    public class ProofOfPaymentsController : ControllerBase
    {
        private readonly IProofOfPaymentService _service;

        public ProofOfPaymentsController(IProofOfPaymentService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var items = await _service.GetAllAsync();
            return Ok(items.ApplyQuery(query,
                nameof(ProofOfPaymentReadDto.Status),
                nameof(ProofOfPaymentReadDto.VerifiedBy)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("by-payment/{paymentId:int}")]
        public async Task<IActionResult> GetByPayment(int paymentId)
            => Ok(await _service.GetByPaymentAsync(paymentId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProofOfPaymentDto dto)
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProofOfPaymentDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");
            var item = await _service.UpdateAsync(dto);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost("{id:int}/verify")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Verify(int id)
        {
            var verifiedBy = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? User.Identity?.Name
                ?? "Unknown";

            var item = await _service.VerifyAsync(id, verifiedBy);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
