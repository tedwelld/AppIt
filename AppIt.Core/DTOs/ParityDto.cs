namespace AppIt.Core.DTOs
{
    public class DebtorLineDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal Outstanding { get; set; }
        public decimal CreditLimit { get; set; }
    }

    public class CreditMemoReadDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string? Notes { get; set; }
    }

    public class CreateCreditMemoDto
    {
        public int ReservationId { get; set; }
        public int? CreditNoteId { get; set; }
        public int? InvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string? Notes { get; set; }
    }

    public class FiscalStatusDto
    {
        public bool ZraEnabled { get; set; }
        public bool DeviceInitialized { get; set; }
        public int PendingInvoices { get; set; }
        public int PendingCreditNotes { get; set; }
    }

    public class HConnectBookingReadDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string SyncStatus { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
