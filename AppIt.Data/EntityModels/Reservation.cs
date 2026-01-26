using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;
using static System.Collections.Specialized.BitVector32;

namespace AppIt.Data.EntityModels
{
    public class Reservation
    {
       
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
        public int ReservationId { get; set; }
        public int? CustomerId { get; set; }
    }
}
