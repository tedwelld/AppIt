using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AppIt.Core.DTOs
{
    public class CompanyDto
    {
        [Required]
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        [Required]
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public string RegNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; }
        public string VatNumber { get; set; }
    }

    public class CreateCompanyDto
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string RegNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; }
        public string VatNumber { get; set; }
    }
    public class UpdateCompanyDto
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string RegNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; }
        public string VatNumber { get; set; }
    }
    public class CompanyDetailsDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public string RegNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; }
        public string VatNumber { get; set; }
    }

    public class CompanyListDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
    }

    public class CompanyDropdownDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
    public class CompanySelectDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
    public class CompanySummaryDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
    }
}
