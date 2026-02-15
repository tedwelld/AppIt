using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] BookingCheckoutRequestDto request)
        {
            var accountId = request.Reservation.AccountId ?? 1;

            try
            {
                var result = await _bookingService.CheckoutAsync(request, accountId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
