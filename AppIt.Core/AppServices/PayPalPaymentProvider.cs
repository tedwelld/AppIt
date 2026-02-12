using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AppIt.Core.Configuration;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace AppIt.Core.Services
{
    public class PayPalPaymentProvider : IPaymentProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;
        private readonly PayPalOptions _options;

        public PayPalPaymentProvider(HttpClient httpClient, IOptions<PaymentProviderOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value.PayPal;
        }

        public string Name => "PayPal";

        public async Task<PaymentProviderResult> ProcessAsync(ProcessPaymentDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ClientId)
                || string.IsNullOrWhiteSpace(_options.ClientSecret)
                || _options.ClientId.Contains("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase)
                || _options.ClientSecret.Contains("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase))
            {
                return new PaymentProviderResult
                {
                    Success = false,
                    Status = "Failed",
                    Message = "PayPal is not configured. Set Payments:PayPal:ClientId and Payments:PayPal:ClientSecret."
                };
            }

            try
            {
                var baseUrl = ResolveBaseUrl();
                var accessToken = await GetAccessTokenAsync(baseUrl, cancellationToken);
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return new PaymentProviderResult
                    {
                        Success = false,
                        Status = "Failed",
                        Message = "Unable to obtain PayPal access token."
                    };
                }

                var payload = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            reference_id = $"INV-{request.InvoiceId}",
                            amount = new
                            {
                                currency_code = request.CurrencyCode.ToUpperInvariant(),
                                value = request.Amount.ToString("0.00", CultureInfo.InvariantCulture)
                            }
                        }
                    },
                    application_context = new
                    {
                        return_url = string.IsNullOrWhiteSpace(request.ReturnUrl) ? "https://example.com/payment/success" : request.ReturnUrl,
                        cancel_url = string.IsNullOrWhiteSpace(request.CancelUrl) ? "https://example.com/payment/cancel" : request.CancelUrl
                    }
                };

                using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders")
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
                };
                createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var response = await _httpClient.SendAsync(createRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new PaymentProviderResult
                    {
                        Success = false,
                        Status = "Failed",
                        Message = $"PayPal order creation failed: {(int)response.StatusCode} {body}"
                    };
                }

                using var doc = JsonDocument.Parse(body);
                var id = doc.RootElement.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? string.Empty : string.Empty;
                var status = doc.RootElement.TryGetProperty("status", out var statusEl) ? statusEl.GetString() ?? "CREATED" : "CREATED";
                string? approveLink = null;
                if (doc.RootElement.TryGetProperty("links", out var linksEl) && linksEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var link in linksEl.EnumerateArray())
                    {
                        if (link.TryGetProperty("rel", out var relEl)
                            && string.Equals(relEl.GetString(), "approve", StringComparison.OrdinalIgnoreCase)
                            && link.TryGetProperty("href", out var hrefEl))
                        {
                            approveLink = hrefEl.GetString();
                            break;
                        }
                    }
                }

                return new PaymentProviderResult
                {
                    Success = true,
                    Status = "Pending",
                    Message = "PayPal order created.",
                    TransactionReference = id,
                    RedirectUrl = approveLink
                };
            }
            catch (Exception ex)
            {
                return new PaymentProviderResult
                {
                    Success = false,
                    Status = "Failed",
                    Message = $"PayPal error: {ex.Message}"
                };
            }
        }

        private async Task<string?> GetAccessTokenAsync(string baseUrl, CancellationToken cancellationToken)
        {
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token")
            {
                Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

            using var tokenResponse = await _httpClient.SendAsync(tokenRequest, cancellationToken);
            var body = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return null;
            }

            using var tokenDoc = JsonDocument.Parse(body);
            return tokenDoc.RootElement.TryGetProperty("access_token", out var tokenEl) ? tokenEl.GetString() : null;
        }

        private string ResolveBaseUrl()
        {
            return _options.Environment.Equals("Production", StringComparison.OrdinalIgnoreCase)
                ? "https://api-m.paypal.com"
                : "https://api-m.sandbox.paypal.com";
        }
    }
}
