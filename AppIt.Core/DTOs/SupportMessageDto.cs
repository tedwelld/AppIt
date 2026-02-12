using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class SupportMessageReadDto
    {
        public int Id { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = "Support";
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSupportMessageDto
    {
        [Required]
        public string FromEmail { get; set; } = string.Empty;
        [Required]
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = "Support";
        [Required]
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
    }

    public class UpdateSupportMessageDto : CreateSupportMessageDto
    {
        [Required]
        public int Id { get; set; }
    }
}
