using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AppIt.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;
        private readonly AppItDbContext _context;

        public ReservationController(IReservationService service, AppItDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryOptions query)
        {
            var reservations = await _service.GetAllAsync();

            return Ok(reservations.ApplyQuery(query,
                nameof(ReservationReadDto.Reference),
                nameof(ReservationReadDto.CustomerEmail),
                nameof(ReservationReadDto.Status),
                nameof(ReservationReadDto.CreatedAt)));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine([FromQuery] int? accountId, [FromQuery] ListQueryOptions query)
        {
            var reservations = accountId.HasValue && accountId.Value > 0
                ? await _service.GetByAccountIdAsync(accountId.Value)
                : Enumerable.Empty<ReservationReadDto>();

            return Ok(reservations.ApplyQuery(query,
                nameof(ReservationReadDto.Reference),
                nameof(ReservationReadDto.CustomerEmail),
                nameof(ReservationReadDto.Status),
                nameof(ReservationReadDto.CreatedAt)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _service.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            return Ok(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reservation = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = reservation.ReservationId }, reservation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            if (dto.ReservationId != 0 && id != dto.ReservationId)
            {
                return BadRequest("ID mismatch");
            }

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var update = new UpdateReservationDto
            {
                ReservationId = id,
                Reference = string.IsNullOrWhiteSpace(dto.Reference) ? existing.Reference : dto.Reference,
                VoucherCode = string.IsNullOrWhiteSpace(dto.VoucherCode) ? existing.VoucherCode : dto.VoucherCode,
                CustomerId = dto.CustomerId ?? existing.CustomerId,
                AccountId = dto.AccountId ?? existing.AccountId,
                AgencyId = dto.AgencyId ?? existing.AgencyId,
                Currency = string.IsNullOrWhiteSpace(dto.Currency) ? existing.Currency : dto.Currency,
                TotalAmount = dto.TotalAmount > 0 ? dto.TotalAmount : existing.TotalAmount,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? existing.Status : dto.Status,
                CustomerEmail = dto.CustomerEmail ?? existing.CustomerEmail,
                ClosingByUserId = dto.ClosingByUserId,
                ClosingByUserName = dto.ClosingByUserName
            };

            if (update.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                var user = await ResolveCurrentUserAsync();
                update.ClosingByUserId = user.Id;
                update.ClosingByUserName = user.DisplayName;
            }

            var reservation = await _service.UpdateAsync(update);
            if (reservation == null)
            {
                return NotFound();
            }

            return Ok(reservation);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var result = await _service.DeleteAsync(id);
            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Success)
            {
                var problem = new ProblemDetails
                {
                    Status = result.HasPaidInvoice ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest,
                    Title = result.HasPaidInvoice ? "Booking cannot be deleted" : "Delete failed",
                    Detail = result.Message,
                    Instance = HttpContext.Request.Path
                };
                problem.Extensions["hasPaidInvoice"] = result.HasPaidInvoice;

                return result.HasPaidInvoice
                    ? Conflict(problem)
                    : BadRequest(problem);
            }

            return NoContent();
        }

        private async Task<(int? Id, string DisplayName)> ResolveCurrentUserAsync()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var email = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);

            if (int.TryParse(userIdValue, out var userId))
            {
                var account = await _context.Accounts
                    .AsNoTracking()
                    .Where(a => a.Id == userId)
                    .Select(a => new { a.Id, a.FirstName, a.LastName, a.Email })
                    .FirstOrDefaultAsync();

                if (account != null)
                {
                    return (account.Id, BuildDisplayName(account.FirstName, account.LastName, account.Email));
                }
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var account = await _context.Accounts
                    .AsNoTracking()
                    .Where(a => a.Email == email)
                    .Select(a => new { a.Id, a.FirstName, a.LastName, a.Email })
                    .FirstOrDefaultAsync();

                if (account != null)
                {
                    return (account.Id, BuildDisplayName(account.FirstName, account.LastName, account.Email));
                }
            }

            return (null, User.Identity?.Name ?? email ?? "Unknown user");
        }

        private static string BuildDisplayName(string? firstName, string? lastName, string email)
        {
            var displayName = $"{firstName} {lastName}".Trim();
            return string.IsNullOrWhiteSpace(displayName) ? email : displayName;
        }
    }
}
