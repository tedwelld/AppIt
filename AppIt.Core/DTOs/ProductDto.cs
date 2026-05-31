using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ProductReadDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Category { get; set; } = "Product";
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ServicePriceReadDto> Prices { get; set; } = new();
    }

    public class CreateProductDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(120)]
        public string Category { get; set; } = "Product";

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePriceUsd { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(120)]
        public string Category { get; set; } = "Product";

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePriceUsd { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
