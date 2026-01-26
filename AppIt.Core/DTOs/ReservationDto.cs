using System;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class ReservationReadDto
    {
        public int ReservationId { get; set; }
        public string? CustomerFirstName { get; set; }
        public string? CustomerLastName { get; set; }
        public string? CustomerIdNumber { get; set; }
        public int? AgencyId { get; set; }
        public int? AgencyConsultantId { get; set; }
        public string? AgencyVoucherReference { get; set; }
        public int? NumberOfPeople { get; set; }
        public int? CurrencyId { get; set; }
        public decimal CurrencyExchangeRate { get; set; }
        public string? Country { get; set; }
        public decimal? Vat { get; set; }
        public bool? IsInvoiced { get; set; }
        public string? Notes { get; set; }
        public int? AnalysisId { get; set; }
        public string? CustomerEmail { get; set; }
        public int? ClosingByUserId { get; set; }
        public string? ClosingByUserName { get; set; }
        public DateTime? ClosingDate { get; set; }
    }

    public class CreateReservationDto
    {
        public string? CustomerFirstName { get; set; }
        public string? CustomerLastName { get; set; }
        public string? CustomerIdNumber { get; set; }
        public int? AgencyId { get; set; }
        public int? AgencyConsultantId { get; set; }
        public string? AgencyVoucherReference { get; set; }
        public int? NumberOfPeople { get; set; }
        public int? CurrencyId { get; set; }
        public decimal CurrencyExchangeRate { get; set; } = 1;
        public string? Country { get; set; }
        public decimal? Vat { get; set; }
        public bool? IsInvoiced { get; set; }
        public string? Notes { get; set; }
        public int? AnalysisId { get; set; }
        public string? CustomerEmail { get; set; }
        public int? ClosingByUserId { get; set; }
        public string? ClosingByUserName { get; set; }
        public DateTime? ClosingDate { get; set; }
    }

    public class UpdateReservationDto : CreateReservationDto
    {
        [Required]
        public int ReservationId { get; set; }
    }
}
