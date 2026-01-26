using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Company
    {
        public int CompanyId { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = null!;

        [MaxLength(500)]
        public string CompanyAddress { get; set; } = null!;

        [EmailAddress]
        public string CompanyEmail { get; set; } = null!;

        [Phone]
        public string CompanyPhone { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string RegNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string AccountNumber { get; set; } = null!;

        [MaxLength(50)]
        public string VatNumber { get; set; } = null!;
    }
}
