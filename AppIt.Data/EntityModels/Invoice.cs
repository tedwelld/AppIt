using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public bool IsFiscalized { get; set; }
        public DateTime? DateFiscalized { get; set; }
        public long? FiscalReceiptNo { get; set; }

        [MaxLength(500)]
        public string? FiscalQrCodeUrl { get; set; }

        [MaxLength(100)]
        public string? FiscalSdcId { get; set; }

        [MaxLength(100)]
        public string? FiscalCisInvoiceNo { get; set; }

        public bool IsReadyToRefiscalize { get; set; }
        public string ReservationSnapShot { get; set; } = string.Empty;
        public string RefiscalizeReservationSnapShot { get; set; } = string.Empty;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<CreditMemo> CreditMemos { get; set; } = new List<CreditMemo>();
    }
}
