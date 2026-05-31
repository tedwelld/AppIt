using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class RefundReadDto
    {
        public int Id { get; set; }
        public int? PaymentId { get; set; }
        public int? InvoiceId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateRefundDto
    {
        public int? PaymentId { get; set; }
        public int? InvoiceId { get; set; }
        [Required] public string Reason { get; set; } = string.Empty;
        [Required] public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
    }

    public class UpdateRefundDto : CreateRefundDto
    {
        [Required] public int Id { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
    }
}
