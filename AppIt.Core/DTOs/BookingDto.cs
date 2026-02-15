using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class BookingCheckoutRequestDto
    {
        [Required]
        public CreateReservationDto Reservation { get; set; } = new();

        [Required]
        public CreateInvoiceDto Invoice { get; set; } = new();

        [Required]
        public ProcessPaymentDto Payment { get; set; } = new();

        public CreateVoucherDto? Voucher { get; set; }
    }

    public class BookingCheckoutResultDto
    {
        public ReservationReadDto Reservation { get; set; } = new();
        public InvoiceReadDto Invoice { get; set; } = new();
        public ProcessPaymentResultDto Payment { get; set; } = new();
        public VoucherReadDto? Voucher { get; set; }
    }
}
