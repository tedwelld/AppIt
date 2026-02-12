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
        public string Currency { get; set; } = "USD";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public string? CustomerEmail { get; set; }
    }

    public class CreateReservationDto
    {
        public string? Reference { get; set; }
        public string? VoucherCode { get; set; }
        public int? CustomerId { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? CustomerEmail { get; set; }
    }

    public class UpdateReservationDto : CreateReservationDto
    {
        [Required]
        public int ReservationId { get; set; }
    }

    public class ReservationDeleteResultDto
    {
        public bool Success { get; set; }
        public bool NotFound { get; set; }
        public bool HasPaidInvoice { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
