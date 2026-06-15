namespace AppIt.Core.Interfaces
{
    public interface IResourceAuthorizationService
    {
        Task<bool> CanAccessReservationAsync(int reservationId, CancellationToken cancellationToken = default);
        Task<int?> GetReservationAccountIdAsync(int reservationId, CancellationToken cancellationToken = default);
        Task<bool> CanAccessInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default);
        Task<bool> CanAccessPaymentAsync(int paymentId, CancellationToken cancellationToken = default);
        Task<bool> CanAccessVoucherAsync(int voucherId, CancellationToken cancellationToken = default);
    }
}
