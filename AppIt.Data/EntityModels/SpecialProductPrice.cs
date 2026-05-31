using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class SpecialProductPrice
    {
        [Key]
        public int Id { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [MaxLength(50)]
        public string? ProductType { get; set; }

        public int? ConsultantId { get; set; }
        public Consultant? Consultant { get; set; }

        public decimal SpecialPrice { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
