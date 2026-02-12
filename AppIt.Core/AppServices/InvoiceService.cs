using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppItDbContext _context;

        public InvoiceService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceReadDto> CreateAsync(CreateInvoiceDto dto)
        {
            var invoice = new Invoice
            {
                ReservationId = dto.ReservationId,
                TotalAmount = dto.TotalAmount,
                CurrencyCode = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                IsPaid = dto.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase),
                IssuedDate = DateTime.UtcNow
            };

            _context.Add(invoice);
            await _context.SaveChangesAsync();

            return ToReadDto(invoice);
        }

        public async Task<InvoiceReadDto?> UpdateAsync(UpdateInvoiceDto dto)
        {
            var invoice = await _context.Set<Invoice>().FindAsync(dto.Id);
            if (invoice == null) return null;

            invoice.ReservationId = dto.ReservationId;
            invoice.TotalAmount = dto.TotalAmount;
            invoice.CurrencyCode = string.IsNullOrWhiteSpace(dto.Currency) ? invoice.CurrencyCode : dto.Currency;
            invoice.Status = string.IsNullOrWhiteSpace(dto.Status) ? invoice.Status : dto.Status;
            invoice.IsPaid = invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase);

            await _context.SaveChangesAsync();
            return ToReadDto(invoice);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoice = await _context.Set<Invoice>().FindAsync(id);
            if (invoice == null) return false;

            _context.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InvoiceReadDto>> GetAllAsync()
        {
            return await _context.Set<Invoice>()
                .AsNoTracking()
                .Select(i => ToReadDto(i))
                .ToListAsync();
        }

        public async Task<InvoiceReadDto?> GetByIdAsync(int id)
        {
            var invoice = await _context.Set<Invoice>()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
            return invoice == null ? null : ToReadDto(invoice);
        }

        public async Task<InvoicePaymentVerificationSummaryDto> VerifyPaymentsAsync(string granularity, DateTime? atUtc = null)
        {
            var (startUtc, endUtc, normalizedGranularity) = ResolveWindow(granularity, atUtc ?? DateTime.UtcNow);
            var invoices = await _context.Set<Invoice>()
                .AsNoTracking()
                .Where(i => i.IssuedDate >= startUtc && i.IssuedDate < endUtc)
                .OrderByDescending(i => i.IssuedDate)
                .ToListAsync();

            var invoiceIds = invoices.Select(i => i.Id).ToList();
            var payments = invoiceIds.Count == 0
                ? new List<Payment>()
                : await _context.Payments
                    .AsNoTracking()
                    .Where(p => invoiceIds.Contains(p.InvoiceId))
                    .Where(p => !p.ProcessedAt.HasValue || (p.ProcessedAt.Value >= startUtc && p.ProcessedAt.Value < endUtc))
                    .ToListAsync();

            var paymentsByInvoiceId = payments
                .GroupBy(p => p.InvoiceId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.ProcessedAt ?? DateTime.MinValue).ToList());

            var items = invoices.Select(invoice =>
            {
                paymentsByInvoiceId.TryGetValue(invoice.Id, out var invoicePayments);
                var paymentRows = invoicePayments ?? new List<Payment>();
                var hasPaymentRecord = paymentRows.Count > 0;
                var hasPaidPayment = paymentRows.Any(p => p.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase));
                var lastPayment = paymentRows.FirstOrDefault();
                var invoiceMarkedPaid = invoice.IsPaid || invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase);
                var isValidated = invoiceMarkedPaid == hasPaidPayment;

                return new InvoicePaymentVerificationItemDto
                {
                    InvoiceId = invoice.Id,
                    ReservationId = invoice.ReservationId,
                    TotalAmount = invoice.TotalAmount,
                    Currency = invoice.CurrencyCode,
                    InvoiceStatus = invoice.Status,
                    IssuedAt = invoice.IssuedDate,
                    HasPaymentRecord = hasPaymentRecord,
                    HasPaidPayment = hasPaidPayment,
                    PaymentRecordCount = paymentRows.Count,
                    LastPaymentStatus = lastPayment?.Status ?? "N/A",
                    LastPaymentAt = lastPayment?.ProcessedAt,
                    IsValidated = isValidated
                };
            }).ToList();

            return new InvoicePaymentVerificationSummaryDto
            {
                Granularity = normalizedGranularity,
                WindowStartUtc = startUtc,
                WindowEndUtc = endUtc,
                GeneratedAtUtc = DateTime.UtcNow,
                TotalInvoices = items.Count,
                PaidInvoices = items.Count(i => i.InvoiceStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)),
                UnpaidInvoices = items.Count(i => !i.InvoiceStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)),
                ValidatedInvoices = items.Count(i => i.IsValidated),
                MismatchedInvoices = items.Count(i => !i.IsValidated),
                MissingPaymentRecords = items.Count(i => i.InvoiceStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) && !i.HasPaymentRecord),
                Items = items
            };
        }

        private static (DateTime startUtc, DateTime endUtc, string granularity) ResolveWindow(string granularity, DateTime dateUtc)
        {
            var normalized = (granularity ?? "day").Trim().ToLowerInvariant();
            var utcDate = DateTime.SpecifyKind(dateUtc, DateTimeKind.Utc);

            return normalized switch
            {
                "moment" => (utcDate.AddMinutes(-1), utcDate.AddMinutes(1), "moment"),
                "day" => (utcDate.Date, utcDate.Date.AddDays(1), "day"),
                "month" => (new DateTime(utcDate.Year, utcDate.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(utcDate.Year, utcDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1), "month"),
                "year" => (new DateTime(utcDate.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(utcDate.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc), "year"),
                _ => (utcDate.Date, utcDate.Date.AddDays(1), "day")
            };
        }

        private static InvoiceReadDto ToReadDto(Invoice invoice)
        {
            return new InvoiceReadDto
            {
                Id = invoice.Id,
                ReservationId = invoice.ReservationId,
                TotalAmount = invoice.TotalAmount,
                Currency = invoice.CurrencyCode,
                Status = invoice.Status,
                IssuedAt = invoice.IssuedDate
            };
        }
    }
}
