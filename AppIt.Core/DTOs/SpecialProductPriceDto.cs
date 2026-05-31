using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class SpecialProductPriceReadDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string? ProductType { get; set; }
        public int? ConsultantId { get; set; }
        public decimal SpecialPrice { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSpecialProductPriceDto
    {
        public int? ProductId { get; set; }
        public string? ProductType { get; set; }
        public int? ConsultantId { get; set; }
        [Required] public decimal SpecialPrice { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        [Required] public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSpecialProductPriceDto : CreateSpecialProductPriceDto
    {
        [Required] public int Id { get; set; }
    }
}
