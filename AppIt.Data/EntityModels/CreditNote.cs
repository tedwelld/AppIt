using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class CreditNote
    {
        [Key]
        public int Id { get; set; }

        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public int? ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        [MaxLength(200)]
        public string Reason { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(200)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
