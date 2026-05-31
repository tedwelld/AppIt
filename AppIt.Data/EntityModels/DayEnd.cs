using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class DayEnd
    {
        [Key]
        public int Id { get; set; }

        public DateTime AuditDate { get; set; }

        [MaxLength(150)]
        public string? OpenedBy { get; set; }

        [MaxLength(150)]
        public string? ClosedBy { get; set; }

        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }

        public decimal TotalRevenue { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Open";

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
