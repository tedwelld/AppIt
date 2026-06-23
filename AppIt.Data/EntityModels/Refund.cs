using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Refund
    {
        [Key]
        public int Id { get; set; }

        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        [MaxLength(200)]
        public string Reason { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(200)]
        public string? ProcessedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsFiscalized { get; set; }
        public DateTime? DateFiscalized { get; set; }
        public long? FiscalReceiptNo { get; set; }

        [MaxLength(500)]
        public string? FiscalQrCodeUrl { get; set; }

        [MaxLength(100)]
        public string? FiscalOriginalReceiptNo { get; set; }

        [MaxLength(100)]
        public string? PaymentProviderRefundId { get; set; }
    }
}
