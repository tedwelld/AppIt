using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class TripAccountReadDto
    {
        public int TripAccountId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string AgentType { get; set; } = "Company";
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string VatNumber { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CreateTripAccountDto
    {
        [Required, MaxLength(200)]
        public string AgentName { get; set; } = string.Empty;

        [MaxLength(40)]
        public string AgentType { get; set; } = "Company";

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string VatNumber { get; set; } = string.Empty;
    }

    public class UpdateTripAccountDto : CreateTripAccountDto
    {
        [Required]
        public int TripAccountId { get; set; }
    }
}
