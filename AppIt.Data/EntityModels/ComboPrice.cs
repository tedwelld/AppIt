using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class ComboPrice
    {
        [Key]
        public int Id { get; set; }

        public int ComboId { get; set; }
        public Combo? Combo { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal UnitPrice { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
