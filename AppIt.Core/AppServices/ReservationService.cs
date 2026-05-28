using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class ReservationService : IReservationService
    {
        private readonly AppItDbContext _context;

        public ReservationService(AppItDbContext context)
        {
            _context = context;
        }

        private DbSet<Reservation> Reservations => _context.Set<Reservation>();

        public async Task<ReservationReadDto> CreateAsync(CreateReservationDto dto)
        {
            var reference = string.IsNullOrWhiteSpace(dto.Reference) ? await BuildUniqueReferenceAsync("RES") : dto.Reference!;
            var voucher = string.IsNullOrWhiteSpace(dto.VoucherCode) ? await BuildUniqueVoucherAsync("RES") : dto.VoucherCode!;

            var reservation = new Reservation
            {
                Reference = reference,
                VoucherCode = voucher,
                CurrencyCode = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency,
                TotalAmount = dto.TotalAmount,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                CreatedDate = DateTime.UtcNow,
                AccountId = dto.AccountId,
                AgencyId = dto.AgencyId,
                CustomerId = dto.CustomerId,
                CustomerEmail = dto.CustomerEmail
            };

            Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return ToReadDto(reservation);
        }

        public async Task<ReservationReadDto?> UpdateAsync(UpdateReservationDto dto)
        {
            var reservation = await Reservations.FindAsync(dto.ReservationId);
            if (reservation == null) return null;

            reservation.Reference = string.IsNullOrWhiteSpace(dto.Reference) ? reservation.Reference : dto.Reference!;
            reservation.VoucherCode = string.IsNullOrWhiteSpace(dto.VoucherCode) ? reservation.VoucherCode : dto.VoucherCode!;
            reservation.CurrencyCode = string.IsNullOrWhiteSpace(dto.Currency) ? reservation.CurrencyCode : dto.Currency;
            reservation.TotalAmount = dto.TotalAmount;
            reservation.Status = string.IsNullOrWhiteSpace(dto.Status) ? reservation.Status : dto.Status;
            reservation.AccountId = dto.AccountId;
            reservation.AgencyId = dto.AgencyId;
            reservation.CustomerId = dto.CustomerId;
            reservation.CustomerEmail = dto.CustomerEmail;

            if (reservation.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                reservation.ClosingDate = DateTime.UtcNow;
                reservation.ClosingByUserId = dto.ClosingByUserId ?? reservation.ClosingByUserId;
                if (!string.IsNullOrWhiteSpace(dto.ClosingByUserName))
                    reservation.ClosingByUserName = dto.ClosingByUserName;
            }

            await _context.SaveChangesAsync();
            return ToReadDto(reservation);
        }

        public async Task<ReservationDeleteResultDto> DeleteAsync(int id)
        {
            var reservation = await Reservations.FindAsync(id);
            if (reservation == null)
            {
                return new ReservationDeleteResultDto
                {
                    Success = false,
                    NotFound = true,
                    Message = "Reservation not found."
                };
            }

            var invoiceIds = await _context.Invoices
                .Where(i => i.ReservationId == id)
                .Select(i => i.Id)
                .ToListAsync();

            var hasPaidInvoice = await _context.Invoices
                .Where(i => i.ReservationId == id)
                .AnyAsync(i => i.IsPaid || i.Status.ToLower() == "paid");

            var hasPaidPayment = invoiceIds.Count > 0 && await _context.Payments
                .Where(p => invoiceIds.Contains(p.InvoiceId))
                .AnyAsync(p => p.Status.ToLower() == "paid");

            if (hasPaidInvoice || hasPaidPayment)
            {
                return new ReservationDeleteResultDto
                {
                    Success = false,
                    HasPaidInvoice = true,
                    Message = "Reservation cannot be deleted because payment has already been made."
                };
            }

            Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return new ReservationDeleteResultDto
            {
                Success = true,
                Message = "Reservation deleted."
            };
        }

        public async Task<ReservationReadDto?> GetByIdAsync(int id)
        {
            var reservation = await Reservations
                .AsNoTracking()
                .Include(r => r.Account)
                .Include(r => r.Customer)
                .Include(r => r.Invoices)
                    .ThenInclude(i => i.Payments)
                .Include(r => r.ServiceItems)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
            return reservation == null ? null : ToReadDto(reservation);
        }

        public async Task<IEnumerable<ReservationReadDto>> GetAllAsync()
        {
            var reservations = await Reservations
                .AsNoTracking()
                .Include(r => r.Account)
                .Include(r => r.Customer)
                .Include(r => r.Invoices)
                    .ThenInclude(i => i.Payments)
                .ToListAsync();
            return reservations.Select(ToReadDto);
        }

        public async Task<IEnumerable<ReservationReadDto>> GetByAccountIdAsync(int accountId)
        {
            var reservations = await Reservations
                .AsNoTracking()
                .Include(r => r.Account)
                .Include(r => r.Customer)
                .Include(r => r.Invoices)
                    .ThenInclude(i => i.Payments)
                .Where(r => r.AccountId == accountId)
                .ToListAsync();
            return reservations.Select(ToReadDto);
        }

        private ReservationReadDto ToReadDto(Reservation r) =>
            ToReadDto(r, r.Invoices?.SelectMany(i => i.Payments ?? Enumerable.Empty<Payment>()));

        private ReservationReadDto ToReadDto(Reservation r, IEnumerable<Payment>? payments)
        {
            var invoice = r.Invoices?
                .OrderByDescending(i => i.IssuedDate)
                .ThenByDescending(i => i.Id)
                .FirstOrDefault();
            var paymentRows = (payments ?? Enumerable.Empty<Payment>())
                .OrderByDescending(p => p.ProcessedAt ?? DateTime.MinValue)
                .ThenByDescending(p => p.Id)
                .ToList();
            var payment = paymentRows.FirstOrDefault();
            var paidAmount = paymentRows
                .Where(p => p.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                    || p.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                    || p.Status.Equals("Captured", StringComparison.OrdinalIgnoreCase))
                .Sum(p => (decimal?)p.Amount) ?? 0;
            var paymentStatus = ResolvePaymentStatus(invoice, payment);

            return new ReservationReadDto
            {
                ReservationId = r.ReservationId,
                Reference = r.Reference,
                VoucherCode = r.VoucherCode,
                CustomerId = r.CustomerId,
                AccountId = r.AccountId,
                AgencyId = r.AgencyId,
                Currency = r.CurrencyCode,
                TotalAmount = r.TotalAmount,
                Status = r.Status,
                CreatedAt = r.CreatedDate,
                CustomerEmail = r.CustomerEmail,

                CustomerFirstName = r.Customer?.FirstName,
                CustomerLastName = r.Customer?.Surname,
                AccountFirstName = r.Account?.FirstName,
                AccountLastName = r.Account?.LastName,
                AgencyConsultantId = r.AgencyConsultantId,
                AgencyVoucherReference = r.AgencyVoucherReference,
                ClosingDate = r.ClosingDate,
                ClosingByUserName = r.ClosingByUserName,

                InvoiceId = invoice?.Id,
                InvoiceTotalAmount = invoice?.TotalAmount,
                InvoiceCurrency = invoice?.CurrencyCode,
                InvoiceStatus = invoice?.Status,

                PaymentStatus = paymentStatus,
                PaymentAmount = paidAmount,

                ServiceItems = r.ServiceItems.Select(item => new BookingServiceItemDto
                {
                    Id = item.Id,
                    ServiceType = item.ServiceType,
                    ServiceId = item.ServiceId,
                    ServiceName = item.ServiceName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Currency = item.Currency
                }).ToList()
            };
        }

        private static string? ResolvePaymentStatus(Invoice? invoice, Payment? payment)
        {
            if (payment != null)
            {
                return payment.Status;
            }

            if (invoice == null)
            {
                return null;
            }

            if (invoice.IsPaid || invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                return "Paid";
            }

            return string.IsNullOrWhiteSpace(invoice.Status) ? "Pending" : invoice.Status;
        }

        private async Task<string> BuildUniqueReferenceAsync(string prefix)
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var reference = BuildReference(prefix);
                if (!await Reservations.AnyAsync(r => r.Reference == reference))
                {
                    return reference;
                }
            }

            throw new InvalidOperationException("Could not generate a unique reservation reference.");
        }

        private async Task<string> BuildUniqueVoucherAsync(string prefix)
        {
            var pattern = $"VCH-{prefix}-";
            var existing = await Reservations
                .Where(r => r.VoucherCode != null && r.VoucherCode.StartsWith(pattern))
                .Select(r => r.VoucherCode!)
                .ToListAsync();

            var maxSeq = 0;
            foreach (var code in existing)
            {
                var suffix = code[pattern.Length..];
                if (int.TryParse(suffix, out var n) && n > maxSeq)
                    maxSeq = n;
            }

            var next = maxSeq + 1;
            return $"VCH-{prefix}-{next:D6}";
        }

        private static string BuildReference(string prefix)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var unique = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(4));
            return $"APP-{prefix}-{stamp}-{unique}";
        }
    }
}
