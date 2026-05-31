using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ProductCategoryReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductCategoryDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductCategoryDto : CreateProductCategoryDto
    {
        [Required] public int Id { get; set; }
    }

    public class ProductSubCategoryReadDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductSubCategoryDto
    {
        [Required] public int CategoryId { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductSubCategoryDto : CreateProductSubCategoryDto
    {
        [Required] public int Id { get; set; }
    }
}
