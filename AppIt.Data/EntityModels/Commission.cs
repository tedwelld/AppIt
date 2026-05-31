using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Commission
    {
        [Key]
        public int Id { get; set; }

        public int? ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public int? ConsultantId { get; set; }
        public Consultant? Consultant { get; set; }

        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime? PaidAt { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
