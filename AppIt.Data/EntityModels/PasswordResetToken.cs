using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [Required]
        [MaxLength(128)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? UsedAtUtc { get; set; }

        [MaxLength(128)]
        public string? CreatedByIp { get; set; }

        [MaxLength(128)]
        public string? UsedByIp { get; set; }
    }
}
