using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace AppIt.Data.EntityModels
{
   
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(120)]
        public string Category { get; set; } = "Product";

        public int? ProductCategoryId { get; set; }

        public ProductCategory? ProductCategory { get; set; }

        /// <summary>Maximum guests (pax) this product can accommodate per booking line.</summary>
        public int? MaxPax { get; set; }

        public decimal BasePriceUsd { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
