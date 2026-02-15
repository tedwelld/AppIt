using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class IdempotencyRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string IdempotencyKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string RequestHash { get; set; } = string.Empty;

        [Required]
        public string ResponseBody { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
