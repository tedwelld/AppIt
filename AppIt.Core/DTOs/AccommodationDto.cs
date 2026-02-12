using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class AccommodationReadDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "Standard";
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateAccommodationDto
    {
        [Required]
        public string Type { get; set; } = "Standard";
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAccommodationDto : CreateAccommodationDto
    {
        [Required]
        public int Id { get; set; }
    }
}
