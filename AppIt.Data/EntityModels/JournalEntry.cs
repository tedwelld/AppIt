using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class JournalEntry
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int VoucherReference { get; set; }
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        [MaxLength(30)]
        public string JournalType { get; set; } = "Reservation";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
    }
}
