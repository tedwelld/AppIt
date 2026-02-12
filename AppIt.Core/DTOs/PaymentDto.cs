using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class PaymentReadDto
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public string Method { get; set; } = "Mastercard";
        public string Status { get; set; } = "Pending";
        public string TransactionReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public DateTime? ProcessedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        [Required]
        public int InvoiceId { get; set; }
        public string Method { get; set; } = "Mastercard";
        public string Status { get; set; } = "Pending";
        public string TransactionReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public DateTime? ProcessedAt { get; set; }
    }

    public class UpdatePaymentDto : CreatePaymentDto
    {
        [Required]
        public int Id { get; set; }
    }
}
