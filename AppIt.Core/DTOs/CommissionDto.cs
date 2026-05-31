using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class CommissionReadDto
    {
        public int Id { get; set; }
        public int? ReservationId { get; set; }
        public int? ConsultantId { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public DateTime? PaidAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCommissionDto
    {
        public int? ReservationId { get; set; }
        public int? ConsultantId { get; set; }
        [Required] public decimal Percentage { get; set; }
        [Required] public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
    }

    public class UpdateCommissionDto : CreateCommissionDto
    {
        [Required] public int Id { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
