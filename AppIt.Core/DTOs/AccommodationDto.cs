using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class AccommodationReadDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "Standard";
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public int GuestCapacity { get; set; }
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ServicePriceReadDto> Prices { get; set; } = new();
    }

    public class CreateAccommodationDto
    {
        [Required]
        public string Type { get; set; } = "Standard";
        public string? Description { get; set; }
        [Range(0, int.MaxValue)]
        public int Capacity { get; set; }
        [Range(1, int.MaxValue)]
        public int GuestCapacity { get; set; } = 1;
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePriceUsd { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAccommodationDto : CreateAccommodationDto
    {
        [Required]
        public int Id { get; set; }
    }
}
