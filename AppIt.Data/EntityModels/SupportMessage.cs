using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class SupportMessage
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(150)]
        public string FromEmail { get; set; } = string.Empty;

        [MaxLength(150)]
        public string ToEmail { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Subject { get; set; } = "Support";

        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
