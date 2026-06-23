using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Data.EntityModels
{
    public class Combo
    {
        [Key]
        public int Id { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ProductCategoryId { get; set; }
        public ProductCategory? ProductCategory { get; set; }

        public int MaxProducts { get; set; } = 2;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ComboProduct> ComboProducts { get; set; } = new List<ComboProduct>();
        public ICollection<ComboPrice> ComboPrices { get; set; } = new List<ComboPrice>();
    }
}
