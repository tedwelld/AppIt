using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;

namespace AppIt.Core.Services
{
    public class ManualPaymentProvider : IPaymentProvider
    {
        public string Name => "Manual";

        public Task<PaymentProviderResult> ProcessAsync(ProcessPaymentDto request, CancellationToken cancellationToken = default)
        {
            var reference = $"MAN-{request.InvoiceId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            return Task.FromResult(new PaymentProviderResult
            {
                Success = true,
                Status = "Paid",
                Message = "Manual payment recorded successfully.",
                TransactionReference = reference
            });
        }
    }
}
