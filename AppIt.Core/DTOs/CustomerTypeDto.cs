using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class CustomerTypeReadDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int Family { get; set; }
        public int CustomerGroup { get; set; }
        public int GroupNumber { get; set; }
        public int TaxationPercentage { get; set; }
        public int Age { get; set; }
        public string? SpecialPrice { get; set; }
        public int Disability { get; set; }
        public int LastSavedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string? Notes { get; set; }
        public IEnumerable<int>? ReservationIds { get; set; }
    }

    public class CreateCustomerTypeDto
    {
        [Required]
        public int CustomerId { get; set; }
        public int Family { get; set; }
        public int CustomerGroup { get; set; }
        public int GroupNumber { get; set; }
        public int TaxationPercentage { get; set; }
        public int Age { get; set; }
        public string? SpecialPrice { get; set; }
        public int Disability { get; set; }
        public int LastSavedBy { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateCustomerTypeDto : CreateCustomerTypeDto
    {
        [Required]
        public int Id { get; set; }
    }
}
