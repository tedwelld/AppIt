using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Consultant
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(150)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

        public decimal CommissionRate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Commission> Commissions { get; set; } = new List<Commission>();
    }
}
