using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class CreditMemo
    {
        [Key]
        public int Id { get; set; }

        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public int? CreditNoteId { get; set; }
        public CreditNote? CreditNote { get; set; }

        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
