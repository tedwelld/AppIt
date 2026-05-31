using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ServicePriceReadDto
    {
        public int Id { get; set; }
        public string ServiceType { get; set; } = "Product";
        public int ServiceId { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServicePriceDto
    {
        [Required]
        public string ServiceType { get; set; } = "Product";

        [Range(1, int.MaxValue)]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateServicePriceDto : CreateServicePriceDto
    {
        [Required]
        public int Id { get; set; }
    }
}
