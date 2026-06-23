using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Accommodation
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(40)]
        public string Type { get; set; } = "Standard";

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ProductCategoryId { get; set; }

        public ProductCategory? ProductCategory { get; set; }

        public int Capacity { get; set; } = 1;

        public int GuestCapacity { get; set; } = 1;

        public decimal BasePriceUsd { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
