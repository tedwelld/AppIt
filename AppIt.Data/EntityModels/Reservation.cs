using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppIt.Data.EntityModels
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        [MaxLength(50)]
        public string Reference { get; set; } = string.Empty;

        [MaxLength(50)]
        public string VoucherCode { get; set; } = string.Empty;

        [MaxLength(10)]
        public string CurrencyCode { get; set; } = "USD";

        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "Enquiry";

        [MaxLength(30)]
        public string PaymentStatus { get; set; } = "NotPaid";

        [MaxLength(30)]
        public string TravelStatus { get; set; } = "NotCheckedIn";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? AccountId { get; set; }
        public Account? Account { get; set; }

        public string? CustomerFirstName { get; set; }
        public string? CustomerLastName { get; set; }
        public string? CustomerIdNumber { get; set; }
        public int? AgencyId { get; set; }
        public int? AgencyConsultantId { get; set; }
        public string? AgencyVoucherReference { get; set; }
        public int? NumberOfPeople { get; set; }
        public int? CurrencyId { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal CurrencyExchangeRate { get; set; } = 1;

        public string? Country { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal? Vat { get; set; }

        public bool? IsInvoiced { get; set; }
        public string? Notes { get; set; }
        public int? AnalysisId { get; set; }
        public string? CustomerEmail { get; set; }
        public int? ClosingByUserId { get; set; }
        public string? ClosingByUserName { get; set; }
        public DateTime? ClosingDate { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? CustomerTypeId { get; set; }
        public CustomerType? CustomerType { get; set; }

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<ReservationServiceItem> ServiceItems { get; set; } = new List<ReservationServiceItem>();
    }
}
