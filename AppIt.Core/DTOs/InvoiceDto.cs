using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class InvoiceReadDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public DateTime IssuedAt { get; set; }
    }

    public class CreateInvoiceDto
    {
        [Required]
        public int ReservationId { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
    }

    public class UpdateInvoiceDto : CreateInvoiceDto
    {
        [Required]
        public int Id { get; set; }
    }
}
