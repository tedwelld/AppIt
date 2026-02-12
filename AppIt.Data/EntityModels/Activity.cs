using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal BasePriceUsd { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
