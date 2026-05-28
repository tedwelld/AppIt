using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class ExchangeRate
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 6)")]
        public decimal Rate { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
