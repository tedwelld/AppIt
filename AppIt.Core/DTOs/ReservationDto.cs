using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ReservationReadDto
    {
        public int ReservationId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string VoucherCode { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public int? AccountId { get; set; }
        public int? AgencyId { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public string? CustomerEmail { get; set; }
        public List<BookingServiceItemDto> ServiceItems { get; set; } = new();

        public string? CustomerFirstName { get; set; }
        public string? CustomerLastName { get; set; }
        public string? AccountFirstName { get; set; }
        public string? AccountLastName { get; set; }
        public int? AgencyConsultantId { get; set; }
        public string? AgencyVoucherReference { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string? ClosingByUserName { get; set; }

        public int? InvoiceId { get; set; }
        public decimal? InvoiceTotalAmount { get; set; }
        public string? InvoiceCurrency { get; set; }
        public string? InvoiceStatus { get; set; }

        public string? PaymentStatus { get; set; }
        public decimal? PaymentAmount { get; set; }
    }

    public class CreateReservationDto
    {
        public string? Reference { get; set; }
        public string? VoucherCode { get; set; }
        public int? CustomerId { get; set; }
        public int? AccountId { get; set; }
        public int? AgencyId { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? CustomerEmail { get; set; }
    }

    public class UpdateReservationDto : CreateReservationDto
    {
        [Required]
        public int ReservationId { get; set; }

        public int? ClosingByUserId { get; set; }
        public string? ClosingByUserName { get; set; }
    }

    public class ReservationDeleteResultDto
    {
        public bool Success { get; set; }
        public bool NotFound { get; set; }
        public bool HasPaidInvoice { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
