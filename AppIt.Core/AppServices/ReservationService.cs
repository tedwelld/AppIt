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
            var reference = string.IsNullOrWhiteSpace(dto.Reference) ? BuildReference("RES") : dto.Reference!;
            var voucher = string.IsNullOrWhiteSpace(dto.VoucherCode) ? BuildVoucher("RES") : dto.VoucherCode!;

            var reservation = new Reservation
            {
                Reference = reference,
                VoucherCode = voucher,
                CurrencyCode = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency,
                TotalAmount = dto.TotalAmount,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                CreatedDate = DateTime.UtcNow,
                AccountId = dto.AccountId,
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
            reservation.CustomerId = dto.CustomerId;
            reservation.CustomerEmail = dto.CustomerEmail;

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
                .AnyAsync(i => i.IsPaid || i.Status.Equals("Paid"));

            var hasPaidPayment = invoiceIds.Count > 0 && await _context.Payments
                .Where(p => invoiceIds.Contains(p.InvoiceId))
                .AnyAsync(p => p.Status.Equals("Paid"));

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
                .FirstOrDefaultAsync(r => r.ReservationId == id);
            return reservation == null ? null : ToReadDto(reservation);
        }

        public async Task<IEnumerable<ReservationReadDto>> GetAllAsync()
        {
            var reservations = await Reservations
                .AsNoTracking()
                .ToListAsync();
            return reservations.Select(ToReadDto);
        }

        public async Task<IEnumerable<ReservationReadDto>> GetByAccountIdAsync(int accountId)
        {
            var reservations = await Reservations
                .AsNoTracking()
                .Where(r => r.AccountId == accountId)
                .ToListAsync();
            return reservations.Select(ToReadDto);
        }

        private ReservationReadDto ToReadDto(Reservation r) => new()
        {
            ReservationId = r.ReservationId,
            Reference = r.Reference,
            VoucherCode = r.VoucherCode,
            CustomerId = r.CustomerId,
            AccountId = r.AccountId,
            Currency = r.CurrencyCode,
            TotalAmount = r.TotalAmount,
            Status = r.Status,
            CreatedAt = r.CreatedDate,
            CustomerEmail = r.CustomerEmail
        };

        private static string BuildReference(string prefix)
        {
            var now = DateTime.UtcNow;
            var stamp = now.ToString("yyyyMMdd");
            var rand = Random.Shared.Next(1000, 9999);
            return $"APP-{prefix}-{stamp}-{rand}";
        }

        private static string BuildVoucher(string prefix)
        {
            var rand = Random.Shared.Next(100000, 999999);
            return $"VCH-{prefix}-{rand}";
        }
    }
}
