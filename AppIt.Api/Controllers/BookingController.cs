using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ICurrentUserService _currentUser;

        public BookingController(IBookingService bookingService, ICurrentUserService currentUser)
        {
            _bookingService = bookingService;
            _currentUser = currentUser;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] BookingCheckoutRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var accountId = _currentUser.ResolveMineAccountId(request.Reservation.AccountId);
            if (!accountId.HasValue || accountId.Value <= 0)
            {
                return BadRequest("AccountId is required.");
            }

            if (!_currentUser.IsStaff
                && request.Reservation.AccountId.HasValue
                && request.Reservation.AccountId.Value != accountId.Value)
            {
                return Forbid();
            }

            request.Reservation.AccountId = accountId.Value;

            try
            {
                var result = await _bookingService.CheckoutAsync(request, accountId.Value);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
