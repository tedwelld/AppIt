using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class AgentProductPrice
    {
        [Key]
        public int Id { get; set; }

        public int? CompanyId { get; set; }
        public Company? Agent { get; set; }

        [MaxLength(50)]
        public string? ProductType { get; set; }

        public int? ProductId { get; set; }

        [MaxLength(180)]
        public string? ProductName { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal? NetRate { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal? RackRate { get; set; }

        public bool IsApproved { get; set; }
        public bool IsAgentApproved { get; set; }
        public bool IsVerified { get; set; }
        public bool Sent { get; set; }
        public bool Query { get; set; }

        public int? YearEffected { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        [MaxLength(500)]
        public string? QueryNotes { get; set; }

        [MaxLength(100)]
        public string? ApprovalKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
