using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class DayEndReadDto
    {
        public int Id { get; set; }
        public DateTime AuditDate { get; set; }
        public string? OpenedBy { get; set; }
        public string? ClosedBy { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Status { get; set; } = "Open";
        public string? Notes { get; set; }
    }

    public class OpenDayEndDto
    {
        [Required] public DateTime AuditDate { get; set; }
        public string? Notes { get; set; }
    }

    public class CloseDayEndDto
    {
        [Required] public int Id { get; set; }
        public string? Notes { get; set; }
    }
}
