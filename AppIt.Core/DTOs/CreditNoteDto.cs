using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class CreditNoteReadDto
    {
        public int Id { get; set; }
        public int? InvoiceId { get; set; }
        public int? ReservationId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCreditNoteDto
    {
        public int? InvoiceId { get; set; }
        public int? ReservationId { get; set; }
        [Required] public string Reason { get; set; } = string.Empty;
        [Required] public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
    }

    public class UpdateCreditNoteDto : CreateCreditNoteDto
    {
        [Required] public int Id { get; set; }
    }
}
