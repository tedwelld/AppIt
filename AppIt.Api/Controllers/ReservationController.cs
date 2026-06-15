using AppIt.Api.Infrastructure;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
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
        private readonly IPricingService _pricing;
        private readonly ICurrentUserService _currentUser;
        private readonly IResourceAuthorizationService _resourceAuth;

        public ReservationController(
            IReservationService service,
            AppItDbContext context,
            IPricingService pricing,
            ICurrentUserService currentUser,
            IResourceAuthorizationService resourceAuth)
        {
            _service = service;
            _context = context;
            _pricing = pricing;
            _currentUser = currentUser;
            _resourceAuth = resourceAuth;
        }

        [HttpGet]
        [Authorize(Roles = "super,admin")]
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
            if (!_currentUser.IsStaff && accountId is > 0 && accountId != _currentUser.UserId)
            {
                return Forbid();
            }

            var resolvedAccountId = _currentUser.ResolveMineAccountId(accountId);
            if (!resolvedAccountId.HasValue)
            {
                return Ok(Enumerable.Empty<ReservationReadDto>().ApplyQuery(query,
                    nameof(ReservationReadDto.Reference),
                    nameof(ReservationReadDto.CustomerEmail),
                    nameof(ReservationReadDto.Status),
                    nameof(ReservationReadDto.CreatedAt)));
            }

            var reservations = await _service.GetByAccountIdAsync(resolvedAccountId.Value);

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

            if (!_currentUser.CanAccessAccount(reservation.AccountId))
            {
                return Forbid();
            }

            return Ok(reservation);
        }

        [HttpPost]
        [Authorize(Roles = "super,admin")]
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

            if (!_currentUser.CanAccessAccount(existing.AccountId))
            {
                return Forbid();
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
        [Authorize(Roles = "super,admin")]
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

        [HttpPost("{id}/service-items")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> AddServiceItem(int id, [FromBody] BookingServiceItemDto dto)
        {
            if (!await _resourceAuth.CanAccessReservationAsync(id))
            {
                return Forbid();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            var quantity = dto.Quantity < 1 ? 1 : dto.Quantity;
            var currency = string.IsNullOrWhiteSpace(dto.Currency) ? reservation.CurrencyCode : dto.Currency;
            var unitPrice = await _pricing.ResolveUnitPriceAsync(
                dto.ServiceType ?? "Product", dto.ServiceId, currency, dto.ActivityDate, reservation.AgencyConsultantId);
            var gross = quantity * unitPrice;
            var discount = Math.Clamp(dto.DiscountPercent ?? 0m, 0m, 100m);
            var lineNet = gross - (gross * discount / 100m);

            var item = new ReservationServiceItem
            {
                ReservationId = id,
                ServiceType = dto.ServiceType ?? "Product",
                ServiceId = dto.ServiceId,
                ServiceName = dto.ServiceName ?? string.Empty,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = lineNet,
                Currency = currency,
                SupplierId = dto.SupplierId,
                AdultPax = dto.AdultPax,
                ChildPax = dto.ChildPax,
                CompPax = dto.CompPax,
                Rooms = dto.Rooms,
                Nights = dto.Nights,
                PickupLocation = dto.PickupLocation,
                DropoffLocation = dto.DropoffLocation,
                ActivityDate = dto.ActivityDate,
                DiscountPercent = dto.DiscountPercent,
                VatPercent = dto.VatPercent,
                CostOfSale = dto.CostOfSale,
                Notes = dto.Notes
            };

            _context.ReservationServiceItems.Add(item);

            var items = await _context.ReservationServiceItems
                .Where(i => i.ReservationId == id)
                .ToListAsync();
            items.Add(item);
            RecalculateReservationTotals(reservation, items);
            await SyncLinkedInvoiceTotalsAsync(id, reservation);

            await _context.SaveChangesAsync();
            return Ok(new { item.Id, item.ReservationId, item.ServiceType, item.ServiceName, item.Quantity, item.UnitPrice, item.TotalPrice, item.Currency });
        }

        [HttpDelete("{id}/service-items/{itemId}")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> RemoveServiceItem(int id, int itemId)
        {
            if (!await _resourceAuth.CanAccessReservationAsync(id))
            {
                return Forbid();
            }

            var item = await _context.ReservationServiceItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ReservationId == id);
            if (item == null) return NotFound();

            _context.ReservationServiceItems.Remove(item);

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                var remaining = await _context.ReservationServiceItems
                    .Where(i => i.ReservationId == id && i.Id != itemId)
                    .ToListAsync();
                RecalculateReservationTotals(reservation, remaining);
                await SyncLinkedInvoiceTotalsAsync(id, reservation);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/close")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> CloseBooking(int id, [FromBody] CloseBookingDto? dto = null)
        {
            var closedBy = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? User.Identity?.Name ?? "Unknown";
            var result = await _service.CloseBookingAsync(id, closedBy);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingDto? dto = null)
        {
            var result = await _service.CancelBookingAsync(id, dto?.Reason);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/open")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> OpenBooking(int id)
        {
            var result = await _service.OpenBookingAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/check-in")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var checkedInBy = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? User.Identity?.Name ?? "Unknown";
            var result = await _service.CheckInBookingAsync(id, checkedInBy);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/clone")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> CloneBooking(int id)
        {
            var clonedBy = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? User.Identity?.Name ?? "Unknown";
            var result = await _service.CloneBookingAsync(id, clonedBy);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("{id}/snapshots")]
        public async Task<IActionResult> GetSnapshots(int id)
        {
            if (!await _resourceAuth.CanAccessReservationAsync(id))
            {
                return Forbid();
            }

            var snapshots = await _service.GetSnapshotsAsync(id);
            return Ok(snapshots);
        }

        [HttpGet("occupancy")]
        [Authorize(Roles = "super,admin")]
        public async Task<IActionResult> GetOccupancy([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 2000 || year > 2100) return BadRequest("Invalid year");
            if (month < 1 || month > 12) return BadRequest("Invalid month");

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);
            var daysInMonth = DateTime.DaysInMonth(year, month);

            var lineItems = await _context.ReservationServiceItems
                .AsNoTracking()
                .Where(i => i.ActivityDate.HasValue && i.ActivityDate.Value >= start && i.ActivityDate.Value < end)
                .Join(
                    _context.Reservations.AsNoTracking(),
                    item => item.ReservationId,
                    reservation => reservation.ReservationId,
                    (item, reservation) => new { item.ActivityDate, reservation.Status, reservation.TotalAmount, reservation.CurrencyCode })
                .ToListAsync();

            var days = Enumerable.Range(1, daysInMonth).Select(day =>
            {
                var date = new DateTime(year, month, day);
                var dayItems = lineItems.Where(r => r.ActivityDate!.Value.Date == date).ToList();
                return new
                {
                    day,
                    date = date.ToString("yyyy-MM-dd"),
                    count = dayItems.Count,
                    revenue = dayItems.Sum(r => r.TotalAmount),
                    statuses = dayItems.GroupBy(r => r.Status)
                        .Select(g => new { status = g.Key, count = g.Count() })
                };
            }).ToList();

            return Ok(new { year, month, totalReservations = lineItems.Count, days });
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

        private async Task SyncLinkedInvoiceTotalsAsync(int reservationId, Reservation reservation)
        {
            var invoice = await _context.Invoices
                .Where(i => i.ReservationId == reservationId)
                .OrderByDescending(i => i.IssuedDate)
                .ThenByDescending(i => i.Id)
                .FirstOrDefaultAsync();
            if (invoice == null)
            {
                return;
            }

            invoice.TotalAmount = reservation.TotalAmount;
            invoice.CurrencyCode = reservation.CurrencyCode;
        }

        private static void RecalculateReservationTotals(Reservation reservation, IEnumerable<ReservationServiceItem> items)
        {
            var net = items.Sum(i => i.TotalPrice);
            var vat = items.Sum(i =>
                decimal.Round(i.TotalPrice * Math.Clamp(i.VatPercent ?? 0m, 0m, 100m) / 100m, 2, MidpointRounding.AwayFromZero));
            reservation.Vat = vat > 0 ? vat : null;
            reservation.TotalAmount = net + vat;
        }
    }
}
