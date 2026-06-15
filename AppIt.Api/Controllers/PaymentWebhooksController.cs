using System.Text;
using System.Text.Json;
using AppIt.Core.Configuration;
using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace AppIt.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/payments/webhooks")]
    public class PaymentWebhooksController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly PaymentProviderOptions _options;
        private readonly ILogger<PaymentWebhooksController> _logger;

        public PaymentWebhooksController(
            IPaymentService paymentService,
            IOptions<PaymentProviderOptions> options,
            ILogger<PaymentWebhooksController> logger)
        {
            _paymentService = paymentService;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body, Encoding.UTF8).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

            Event stripeEvent;
            try
            {
                if (!string.IsNullOrWhiteSpace(_options.Stripe.WebhookSecret)
                    && !_options.Stripe.WebhookSecret.Contains("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(signature))
                {
                    stripeEvent = EventUtility.ConstructEvent(json, signature, _options.Stripe.WebhookSecret);
                }
                else
                {
                    stripeEvent = EventUtility.ParseEvent(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
                return BadRequest();
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session != null)
                {
                    await _paymentService.CompletePendingPaymentByReferenceAsync(session.Id);
                }
            }

            return Ok();
        }

        [HttpPost("paypal")]
        public async Task<IActionResult> PayPalWebhook([FromBody] JsonElement payload)
        {
            try
            {
                var eventType = payload.TryGetProperty("event_type", out var typeEl)
                    ? typeEl.GetString()
                    : null;

                if (eventType is "CHECKOUT.ORDER.APPROVED" or "PAYMENT.CAPTURE.COMPLETED")
                {
                    var reference = ExtractPayPalReference(payload);
                    if (!string.IsNullOrWhiteSpace(reference))
                    {
                        await _paymentService.CompletePendingPaymentByReferenceAsync(reference);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PayPal webhook processing failed.");
                return BadRequest();
            }

            return Ok();
        }

        private static string? ExtractPayPalReference(JsonElement payload)
        {
            if (payload.TryGetProperty("resource", out var resource))
            {
                if (resource.TryGetProperty("id", out var id))
                {
                    return id.GetString();
                }

                if (resource.TryGetProperty("supplementary_data", out var supplementary)
                    && supplementary.TryGetProperty("related_ids", out var related)
                    && related.TryGetProperty("order_id", out var orderId))
                {
                    return orderId.GetString();
                }
            }

            return null;
        }
    }
}
