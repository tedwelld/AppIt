using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingCheckoutResultDto> CheckoutAsync(BookingCheckoutRequestDto request, int accountId);
    }
}
