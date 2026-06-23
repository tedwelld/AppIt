using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ActivityReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? MaxPax { get; set; }
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ServicePriceReadDto> Prices { get; set; } = new();
    }

    public class CreateActivityDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ProductCategoryId { get; set; }
        public int? MaxPax { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateActivityDto : CreateActivityDto
    {
        [Required]
        public int Id { get; set; }
    }
}
