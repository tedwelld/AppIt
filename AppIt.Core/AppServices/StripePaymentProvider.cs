using AppIt.Core.Configuration;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace AppIt.Core.Services
{
    public class StripePaymentProvider : IPaymentProvider
    {
        private readonly StripeOptions _options;

        public StripePaymentProvider(IOptions<PaymentProviderOptions> options)
        {
            _options = options.Value.Stripe;
        }

        public string Name => "Stripe";

        public async Task<PaymentProviderResult> ProcessAsync(ProcessPaymentDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.SecretKey) || _options.SecretKey.Contains("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase))
            {
                return new PaymentProviderResult
                {
                    Success = false,
                    Status = "Failed",
                    Message = "Stripe is not configured. Set Payments:Stripe:SecretKey."
                };
            }

            try
            {
                StripeConfiguration.ApiKey = _options.SecretKey;
                var amountInMinorUnits = (long)Math.Round(request.Amount * 100m, MidpointRounding.AwayFromZero);

                var service = new SessionService();
                var session = await service.CreateAsync(
                    new SessionCreateOptions
                    {
                        Mode = "payment",
                        SuccessUrl = string.IsNullOrWhiteSpace(request.ReturnUrl) ? "https://example.com/payment/success" : request.ReturnUrl,
                        CancelUrl = string.IsNullOrWhiteSpace(request.CancelUrl) ? "https://example.com/payment/cancel" : request.CancelUrl,
                        LineItems = new List<SessionLineItemOptions>
                        {
                            new()
                            {
                                Quantity = 1,
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    Currency = request.CurrencyCode.ToLowerInvariant(),
                                    UnitAmount = amountInMinorUnits,
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = $"Invoice {request.InvoiceId}"
                                    }
                                }
                            }
                        }
                    },
                    cancellationToken: cancellationToken
                );

                return new PaymentProviderResult
                {
                    Success = true,
                    Status = "Pending",
                    Message = "Stripe checkout session created.",
                    TransactionReference = session.Id,
                    RedirectUrl = session.Url
                };
            }
            catch (StripeException ex)
            {
                return new PaymentProviderResult
                {
                    Success = false,
                    Status = "Failed",
                    Message = $"Stripe error: {ex.StripeError?.Message ?? ex.Message}"
                };
            }
        }
    }
}
