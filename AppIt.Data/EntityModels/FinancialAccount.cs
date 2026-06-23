using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class FinancialAccount
    {
        [Key]
        public int Id { get; set; }

        public int Order { get; set; }

        [MaxLength(50)]
        public string? PastelRef { get; set; }

        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal Balance { get; set; }

        [MaxLength(30)]
        public string AccountType { get; set; } = "Income";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
    }
}
