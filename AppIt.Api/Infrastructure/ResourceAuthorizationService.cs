using AppIt.Core.Interfaces;
using AppIt.Data;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Api.Infrastructure
{
    public class ResourceAuthorizationService : IResourceAuthorizationService
    {
        private readonly AppItDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public ResourceAuthorizationService(AppItDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int?> GetReservationAccountIdAsync(int reservationId, CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.ReservationId == reservationId)
                .Select(r => r.AccountId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> CanAccessReservationAsync(int reservationId, CancellationToken cancellationToken = default)
        {
            var accountId = await GetReservationAccountIdAsync(reservationId, cancellationToken);
            return _currentUser.CanAccessAccount(accountId);
        }

        public async Task<bool> CanAccessInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
        {
            var accountId = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Id == invoiceId)
                .Select(i => i.Reservation!.AccountId)
                .FirstOrDefaultAsync(cancellationToken);

            return _currentUser.CanAccessAccount(accountId);
        }

        public async Task<bool> CanAccessPaymentAsync(int paymentId, CancellationToken cancellationToken = default)
        {
            var accountId = await _context.Payments
                .AsNoTracking()
                .Where(p => p.Id == paymentId)
                .Select(p => p.Invoice!.Reservation!.AccountId)
                .FirstOrDefaultAsync(cancellationToken);

            return _currentUser.CanAccessAccount(accountId);
        }

        public async Task<bool> CanAccessVoucherAsync(int voucherId, CancellationToken cancellationToken = default)
        {
            var reservationId = await _context.Vouchers
                .AsNoTracking()
                .Where(v => v.Id == voucherId)
                .Select(v => v.ReservationId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!reservationId.HasValue) return _currentUser.IsStaff;

            return await CanAccessReservationAsync(reservationId.Value, cancellationToken);
        }
    }
}
