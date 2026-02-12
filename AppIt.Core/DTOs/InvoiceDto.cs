using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppIt.Core.DTOs
{
    public class InvoiceReadDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
        public DateTime IssuedAt { get; set; }
    }

    public class CreateInvoiceDto
    {
        [Required]
        public int ReservationId { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending";
    }

    public class UpdateInvoiceDto : CreateInvoiceDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class InvoicePaymentVerificationItemDto
    {
        public int InvoiceId { get; set; }
        public int ReservationId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string InvoiceStatus { get; set; } = "Pending";
        public DateTime IssuedAt { get; set; }
        public bool HasPaymentRecord { get; set; }
        public bool HasPaidPayment { get; set; }
        public int PaymentRecordCount { get; set; }
        public string LastPaymentStatus { get; set; } = "N/A";
        public DateTime? LastPaymentAt { get; set; }
        public bool IsValidated { get; set; }
    }

    public class InvoicePaymentVerificationSummaryDto
    {
        public string Granularity { get; set; } = "day";
        public DateTime WindowStartUtc { get; set; }
        public DateTime WindowEndUtc { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int UnpaidInvoices { get; set; }
        public int ValidatedInvoices { get; set; }
        public int MismatchedInvoices { get; set; }
        public int MissingPaymentRecords { get; set; }
        public List<InvoicePaymentVerificationItemDto> Items { get; set; } = new();
    }
}


