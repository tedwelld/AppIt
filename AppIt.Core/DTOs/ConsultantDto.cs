using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ConsultantReadDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? CompanyId { get; set; }
        public decimal CommissionRate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateConsultantDto
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? CompanyId { get; set; }
        public decimal CommissionRate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateConsultantDto : CreateConsultantDto
    {
        [Required] public int Id { get; set; }
    }
}
