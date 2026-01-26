using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class CustomerReadDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? IdType { get; set; }
        public string? Nationality { get; set; }
        public string? Dob { get; set; }
        public string? Profession { get; set; }
        public string? ProxyName { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public byte Image { get; set; }
        public string? TaxCategory { get; set; }
        public string? Region { get; set; }
        public int DurationOfStayDays { get; set; }

        public int LastSavedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string? Notes { get; set; }

        public int? CustomerTypeId { get; set; }
        public IEnumerable<int>? ReservationIds { get; set; }
    }

    public class CreateCustomerDto
    {
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? IdType { get; set; }
        public string? Nationality { get; set; }
        public string? Dob { get; set; }
        public string? Profession { get; set; }
        public string? ProxyName { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public byte Image { get; set; }
        public string? TaxCategory { get; set; }
        public string? Region { get; set; }
        public int DurationOfStayDays { get; set; }
       
        public int LastSavedBy { get; set; }
        public string? Notes { get; set; }
        public int? CustomerTypeId { get; set; }
    }

    public class UpdateCustomerDto : CreateCustomerDto
    {
        [Required]
        public int Id { get; set; }
    }
}
