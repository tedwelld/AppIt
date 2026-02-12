using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IPaymentProvider
    {
        string Name { get; }
        Task<PaymentProviderResult> ProcessAsync(ProcessPaymentDto request, CancellationToken cancellationToken = default);
    }

    public class PaymentProviderResult
    {
        public bool Success { get; set; }
        public string Status { get; set; } = "Pending";
        public string Message { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public string? RedirectUrl { get; set; }
    }
}
