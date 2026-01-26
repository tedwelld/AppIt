using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class CompanyReadDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string CompanyAddress { get; set; } = null!;
        public string CompanyEmail { get; set; } = null!;
        public string CompanyPhone { get; set; } = null!;
        public string RegNumber { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string VatNumber { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CreateCompanyDto
    {
        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = null!;

        [MaxLength(500)]
        public string CompanyAddress { get; set; } = null!;

        [EmailAddress]
        public string CompanyEmail { get; set; } = null!;

        [Phone]
        public string CompanyPhone { get; set; } = null!;

        [MaxLength(50)]
        public string RegNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string AccountNumber { get; set; } = null!;

        [MaxLength(50)]
        public string VatNumber { get; set; } = null!;
    }

    public class UpdateCompanyDto : CreateCompanyDto
    {
        [Required]
        public int CompanyId { get; set; }
    }
}
