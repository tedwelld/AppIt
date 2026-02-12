using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        [MaxLength(40)]
        public string Method { get; set; } = "Mastercard";

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [MaxLength(60)]
        public string TransactionReference { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        public DateTime? ProcessedAt { get; set; }
    }
}
