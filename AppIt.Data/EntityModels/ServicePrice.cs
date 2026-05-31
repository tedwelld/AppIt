using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class ServicePrice
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(30)]
        public string ServiceType { get; set; } = "Product";

        public int ServiceId { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        public decimal UnitPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
