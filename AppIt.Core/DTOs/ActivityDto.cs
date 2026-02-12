using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ActivityReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateActivityDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateActivityDto : CreateActivityDto
    {
        [Required]
        public int Id { get; set; }
    }
}
