using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class VsdcCodeEntry
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string CodeType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    }
}
