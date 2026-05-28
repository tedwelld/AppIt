using System;
using System.Collections.Generic;

namespace AppIt.Data.EntityModels
{
    public class Invoice
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public bool IsPaid { get; set; }
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
