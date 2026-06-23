using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class JournalEntryLine
    {
        [Key]
        public int Id { get; set; }

        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }

        public int FinancialAccountId { get; set; }
        public FinancialAccount? Account { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string EntryType { get; set; } = "Debit";
    }
}
