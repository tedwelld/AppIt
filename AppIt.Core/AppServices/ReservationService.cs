using System.Text.Json;
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
        private readonly ICommissionService _commissionService;

        public ReservationService(AppItDbContext context, ICommissionService commissionService)
        {
            _context = context;
            _commissionService = commissionService;
        }

        private DbSet<Reservation> Reservations => _context.Set<Reservation>();

        public async Task<ReservationReadDto> CreateAsync(CreateReservationDto dto)
        {
            var reference = !string.IsNullOrWhiteSpace(dto.Reference)
                ? dto.Reference!.Trim()
                : !string.IsNullOrWhiteSpace(dto.AgencyVoucherReference)
                    ? dto.AgencyVoucherReference!.Trim()
                    : await BuildUniqueReferenceAsync("RES");
            var voucher = string.IsNullOrWhiteSpace(dto.VoucherCode) ? await BuildUniqueVoucherAsync() : dto.VoucherCode!.Trim();

            var reservation = new Reservation
            {
                Reference = reference,
                VoucherCode = voucher,
                CurrencyCode = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency,
                TotalAmount = dto.TotalAmount,
                Vat = dto.Vat,
                CurrencyExchangeRate = dto.CurrencyExchangeRate is > 0 ? dto.CurrencyExchangeRate.Value : 1m,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                PaymentStatus = string.IsNullOrWhiteSpace(dto.PaymentStatus) ? "NotPaid" : dto.PaymentStatus,
                CreatedDate = DateTime.UtcNow,
                AccountId = dto.AccountId,
                AgencyId = dto.AgencyId,
                AgencyConsultantId = dto.AgencyConsultantId,
                AgencyVoucherReference = dto.AgencyVoucherReference,
                CustomerId = dto.CustomerId,
                CustomerEmail = dto.CustomerEmail,
                Country = dto.Country,
                Notes = dto.Notes
            };

            Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            await WriteSnapshotAsync(reservation.ReservationId, "Created", null);
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
            if (dto.AgencyConsultantId.HasValue) reservation.AgencyConsultantId = dto.AgencyConsultantId;
            if (!string.IsNullOrWhiteSpace(dto.AgencyVoucherReference)) reservation.AgencyVoucherReference = dto.AgencyVoucherReference;
            if (!string.IsNullOrWhiteSpace(dto.Country)) reservation.Country = dto.Country;
            if (!string.IsNullOrWhiteSpace(dto.Notes)) reservation.Notes = dto.Notes;

            if (reservation.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                reservation.ClosingDate = DateTime.UtcNow;
                reservation.ClosingByUserId = dto.ClosingByUserId ?? reservation.ClosingByUserId;
                if (!string.IsNullOrWhiteSpace(dto.ClosingByUserName))
                    reservation.ClosingByUserName = dto.ClosingByUserName;
            }

            await _context.SaveChangesAsync();
            await WriteSnapshotAsync(reservation.ReservationId, "Updated", dto.ClosingByUserName);
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
            var paidAmount = paymentRows
                .Where(p => p.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                    || p.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                    || p.Status.Equals("Captured", StringComparison.OrdinalIgnoreCase))
                .Sum(p => (decimal?)p.Amount) ?? 0;
            var paymentStatus = ResolvePaymentStatus(invoice, paidAmount);

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
                TravelStatus = r.TravelStatus,

                ServiceItems = r.ServiceItems.Select(item => new BookingServiceItemDto
                {
                    Id = item.Id,
                    ServiceType = item.ServiceType,
                    ServiceId = item.ServiceId,
                    ServiceName = item.ServiceName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Currency = item.Currency,
                    SupplierId = item.SupplierId,
                    AdultPax = item.AdultPax,
                    ChildPax = item.ChildPax,
                    CompPax = item.CompPax,
                    Rooms = item.Rooms,
                    Nights = item.Nights,
                    PickupLocation = item.PickupLocation,
                    DropoffLocation = item.DropoffLocation,
                    ActivityDate = item.ActivityDate,
                    DiscountPercent = item.DiscountPercent,
                    VatPercent = item.VatPercent,
                    CostOfSale = item.CostOfSale,
                    Notes = item.Notes
                }).ToList()
            };
        }

        private static string ResolvePaymentStatus(Invoice? invoice, decimal paidAmount)
        {
            if (invoice == null || paidAmount <= 0) return "NotPaid";

            // FullyPaid once the summed captured payments cover the invoice; otherwise a deposit.
            return paidAmount >= invoice.TotalAmount ? "FullyPaid" : "Deposited";
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

        private async Task<string> BuildUniqueVoucherAsync()
        {
            var reservationCodes = await Reservations
                .Where(r => r.VoucherCode != null)
                .Select(r => r.VoucherCode!)
                .ToListAsync();
            var voucherCodes = await _context.Vouchers
                .Where(v => v.Code != null)
                .Select(v => v.Code)
                .ToListAsync();

            var existing = reservationCodes.Concat(voucherCodes).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var maxSeq = 0;
            foreach (var code in existing)
            {
                var digits = new string(code.Where(char.IsDigit).ToArray());
                if (int.TryParse(digits, out var n) && n > maxSeq)
                    maxSeq = n;
            }

            for (var attempt = 0; attempt < 10; attempt++)
            {
                var candidate = $"{maxSeq + 1 + attempt:D5}";
                if (!existing.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException("Could not generate a unique voucher code.");
        }

        private static string BuildReference(string prefix)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var unique = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(4));
            return $"APP-{prefix}-{stamp}-{unique}";
        }

        public async Task<ReservationReadDto?> CloseBookingAsync(int id, string closedBy)
        {
            var reservation = await Reservations
                .Include(r => r.Invoices)
                .Include(r => r.ServiceItems)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
            if (reservation == null) return null;

            reservation.Status = "Closed";
            reservation.TravelStatus = "Travelled";
            reservation.IsInvoiced = true;
            reservation.ClosingDate = DateTime.UtcNow;
            reservation.ClosingByUserName = closedBy;

            if (!reservation.Invoices.Any())
            {
                _context.Invoices.Add(new Invoice
                {
                    ReservationId = id,
                    TotalAmount = reservation.TotalAmount,
                    CurrencyCode = reservation.CurrencyCode,
                    Status = "Pending",
                    IssuedDate = DateTime.UtcNow
                });
            }

            await UpdateVoucherStatusAsync(id, "Redeemed");
            await AccrueCommissionAsync(reservation);
            await _context.SaveChangesAsync();
            await WriteSnapshotAsync(id, "Closed", closedBy);
            return await GetByIdAsync(id);
        }

        private async Task AccrueCommissionAsync(Reservation reservation)
        {
            if (!reservation.AgencyConsultantId.HasValue) return;
            var exists = await _context.Commissions.AnyAsync(c => c.ReservationId == reservation.ReservationId);
            if (exists) return;

            var consultant = await _context.Consultants.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == reservation.AgencyConsultantId);
            if (consultant == null || consultant.CommissionRate <= 0) return;

            await _commissionService.CreateAsync(new CreateCommissionDto
            {
                ReservationId = reservation.ReservationId,
                ConsultantId = consultant.Id,
                Percentage = consultant.CommissionRate,
                Amount = decimal.Round(reservation.TotalAmount * consultant.CommissionRate / 100m, 2),
                CurrencyCode = reservation.CurrencyCode,
                Status = "Accrued",
                Notes = "Auto-accrued on close"
            });
        }

        public async Task<ReservationReadDto?> CancelBookingAsync(int id, string? reason = null)
        {
            var reservation = await Reservations
                .Include(r => r.Invoices)
                    .ThenInclude(i => i.Payments)
                .FirstOrDefaultAsync(r => r.ReservationId == id);
            if (reservation == null) return null;

            var invoice = reservation.Invoices?
                .OrderByDescending(i => i.IssuedDate)
                .ThenByDescending(i => i.Id)
                .FirstOrDefault();
            var paidAmount = (invoice?.Payments ?? Enumerable.Empty<Payment>())
                .Where(p => p.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                    || p.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                    || p.Status.Equals("Captured", StringComparison.OrdinalIgnoreCase))
                .Sum(p => (decimal?)p.Amount) ?? 0;
            var computedStatus = ResolvePaymentStatus(invoice, paidAmount);
            reservation.PaymentStatus = computedStatus;

            var alreadyPaid = computedStatus == "FullyPaid";
            reservation.Status = "Cancelled";
            if (!alreadyPaid) reservation.PaymentStatus = "NotPaid";
            if (!string.IsNullOrWhiteSpace(reason))
            {
                reservation.Notes = string.IsNullOrWhiteSpace(reservation.Notes)
                    ? reason.Trim()
                    : $"{reservation.Notes.Trim()}\nCancelled: {reason.Trim()}";
            }

            await UpdateVoucherStatusAsync(id, "Cancelled");
            await _context.SaveChangesAsync();
            await WriteSnapshotAsync(id, "Cancelled", reason);
            return await GetByIdAsync(id);
        }

        public async Task<ReservationReadDto?> OpenBookingAsync(int id)
        {
            var reservation = await Reservations.FindAsync(id);
            if (reservation == null) return null;

            reservation.Status = "Confirmed";
            reservation.TravelStatus = "NotCheckedIn";
            reservation.IsInvoiced = false;
            reservation.ClosingDate = null;
            reservation.ClosingByUserName = null;

            await UpdateVoucherStatusAsync(id, "Active");
            await _context.SaveChangesAsync();
            await WriteSnapshotAsync(id, "Opened", null);
            return await GetByIdAsync(id);
        }

        public async Task<ReservationReadDto?> CheckInBookingAsync(int id, string checkedInBy)
        {
            var reservation = await Reservations.FindAsync(id);
            if (reservation == null) return null;

            // Travel lifecycle: NotCheckedIn -> CheckedIn -> Travelled (Travelled is set on close).
            reservation.TravelStatus = "CheckedIn";

            await UpdateVoucherStatusAsync(id, "Redeemed");
            await _context.SaveChangesAsync();
            await WriteSnapshotAsync(id, "CheckedIn", checkedInBy);
            return await GetByIdAsync(id);
        }

        private async Task UpdateVoucherStatusAsync(int reservationId, string status)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.ReservationId == reservationId);
            if (voucher == null) return;

            voucher.Status = status;
            if (status.Equals("Redeemed", StringComparison.OrdinalIgnoreCase))
            {
                voucher.RedeemedDate ??= DateTime.UtcNow;
            }
            else if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                voucher.RedeemedDate = null;
            }
        }

        public async Task<ReservationReadDto?> CloneBookingAsync(int id, string clonedBy)
        {
            var source = await Reservations
                .Include(r => r.ServiceItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReservationId == id);
            if (source == null) return null;

            var reference = await BuildUniqueReferenceAsync("RES");
            var voucher = await BuildUniqueVoucherAsync();

            var clone = new Reservation
            {
                Reference = reference,
                VoucherCode = voucher,
                CurrencyCode = source.CurrencyCode,
                TotalAmount = source.TotalAmount,
                Status = "Enquiry",
                PaymentStatus = "NotPaid",
                TravelStatus = "NotCheckedIn",
                CreatedDate = DateTime.UtcNow,
                AccountId = source.AccountId,
                AgencyId = source.AgencyId,
                AgencyConsultantId = source.AgencyConsultantId,
                CustomerId = source.CustomerId,
                CustomerEmail = source.CustomerEmail,
                NumberOfPeople = source.NumberOfPeople,
                Notes = source.Notes,
                Country = source.Country
            };

            Reservations.Add(clone);
            await _context.SaveChangesAsync();

            var clonedItems = source.ServiceItems.Select(item => new ReservationServiceItem
            {
                ReservationId = clone.ReservationId,
                ServiceType = item.ServiceType,
                ServiceId = item.ServiceId,
                ServiceName = item.ServiceName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                Currency = item.Currency,
                SupplierId = item.SupplierId,
                AdultPax = item.AdultPax,
                ChildPax = item.ChildPax,
                CompPax = item.CompPax,
                Rooms = item.Rooms,
                Nights = item.Nights,
                PickupLocation = item.PickupLocation,
                DropoffLocation = item.DropoffLocation,
                ActivityDate = item.ActivityDate,
                DiscountPercent = item.DiscountPercent,
                VatPercent = item.VatPercent,
                CostOfSale = item.CostOfSale,
                Notes = item.Notes
            }).ToList();

            _context.ReservationServiceItems.AddRange(clonedItems);
            await _context.SaveChangesAsync();

            await WriteSnapshotAsync(id, "Cloned", clonedBy);
            await WriteSnapshotAsync(clone.ReservationId, "Created", clonedBy);
            return await GetByIdAsync(clone.ReservationId);
        }

        public async Task<IEnumerable<ReservationSnapshotDto>> GetSnapshotsAsync(int id)
        {
            return await _context.ReservationSnapshots
                .AsNoTracking()
                .Where(s => s.ReservationId == id)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ReservationSnapshotDto
                {
                    Id = s.Id,
                    ReservationId = s.ReservationId,
                    SnapshotType = s.SnapshotType,
                    CreatedBy = s.CreatedBy,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();
        }

        private async Task WriteSnapshotAsync(int reservationId, string snapshotType, string? createdBy)
        {
            var reservation = await Reservations
                .AsNoTracking()
                .Include(r => r.ServiceItems)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            var json = reservation == null ? "{}" : JsonSerializer.Serialize(new
            {
                reservation.ReservationId,
                reservation.Reference,
                reservation.VoucherCode,
                reservation.Status,
                reservation.PaymentStatus,
                reservation.TravelStatus,
                reservation.TotalAmount,
                reservation.CurrencyCode,
                reservation.IsInvoiced,
                reservation.ClosingDate,
                reservation.AccountId,
                reservation.CustomerId
            });

            _context.ReservationSnapshots.Add(new ReservationSnapshot
            {
                ReservationId = reservationId,
                SnapshotType = snapshotType,
                SnapshotJson = json,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}
