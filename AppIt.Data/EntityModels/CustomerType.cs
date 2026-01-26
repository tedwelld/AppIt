using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityModels
{
    public class CustomerType
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int Family { get; set; }
        public int CustomerGroup { get; set; }
        public int GroupNumber { get; set; }
        public int TaxationPercentage { get; set; }
        public int Age { get; set; }
        public string? SpecialPice { get; set; }
        public int Disability { get; set; }
        public int LastSavedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public string? Notes { get; set; }
        public Customer? Customer { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}
