using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class BookingCheckoutRequestDto
    {
        public int? CustomerId { get; set; }
        public int? TripAccountId { get; set; }
        public CreateCustomerDto? Customer { get; set; }

        [Required]
        public CreateReservationDto Reservation { get; set; } = new();

        [Required]
        public CreateInvoiceDto Invoice { get; set; } = new();

        [Required]
        public ProcessPaymentDto Payment { get; set; } = new();

        public CreateVoucherDto? Voucher { get; set; }
        public List<BookingServiceItemDto> ServiceItems { get; set; } = new();
    }

    public class BookingCheckoutResultDto
    {
        public CustomerReadDto? Customer { get; set; }
        public ReservationReadDto Reservation { get; set; } = new();
        public InvoiceReadDto Invoice { get; set; } = new();
        public ProcessPaymentResultDto Payment { get; set; } = new();
        public VoucherReadDto? Voucher { get; set; }
        public List<BookingServiceItemDto> ServiceItems { get; set; } = new();
    }

    public class BookingServiceItemDto
    {
        public int? Id { get; set; }

        [Required]
        public string ServiceType { get; set; } = "Product";

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public string ServiceName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "USD";

        public int? SupplierId { get; set; }
        public int? AdultPax { get; set; }
        public int? ChildPax { get; set; }
        public int? CompPax { get; set; }
        public int? Rooms { get; set; }
        public int? Nights { get; set; }
        public string? PickupLocation { get; set; }
        public string? DropoffLocation { get; set; }
        public DateTime? ActivityDate { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? VatPercent { get; set; }
        public decimal? CostOfSale { get; set; }
        public string? Notes { get; set; }
    }

    public class CloseBookingDto
    {
        [Required]
        public int ReservationId { get; set; }
        public string? Notes { get; set; }
    }

    public class CancelBookingDto
    {
        [Required]
        public int ReservationId { get; set; }
        public string? Reason { get; set; }
    }

    public class ReservationSnapshotDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string SnapshotType { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
