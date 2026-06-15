using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ProcessPaymentDto
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public string Method { get; set; } = "Mastercard";

        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string? IdempotencyKey { get; set; }
        public string? TransactionReference { get; set; }
    }

    public class ProcessPaymentResultDto
    {
        public bool Success { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string TransactionReference { get; set; } = string.Empty;
        public string? RedirectUrl { get; set; }
        public int? PaymentId { get; set; }
    }
}
