using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class BankNoteDetail
    {
        [Key]
        public int Id { get; set; }

        public DateTime CashUpDate { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        [MaxLength(20)]
        public string Denomination { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(150)]
        public string? EnteredBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
