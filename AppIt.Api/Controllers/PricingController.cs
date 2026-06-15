using AppIt.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/pricing")]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricing;

        public PricingController(IPricingService pricing)
        {
            _pricing = pricing;
        }

        [HttpGet("quote")]
        public async Task<IActionResult> GetQuote(
            [FromQuery] string serviceType,
            [FromQuery] int serviceId,
            [FromQuery] string currency = "USD",
            [FromQuery] DateTime? date = null,
            [FromQuery] int? consultantId = null)
        {
            if (serviceId <= 0)
            {
                return BadRequest("serviceId is required.");
            }

            if (string.IsNullOrWhiteSpace(serviceType))
            {
                return BadRequest("serviceType is required.");
            }

            try
            {
                var unitPrice = await _pricing.ResolveUnitPriceAsync(serviceType, serviceId, currency, date, consultantId);
                var rate = await _pricing.GetEffectiveRateAsync(currency, date);
                return Ok(new
                {
                    serviceType,
                    serviceId,
                    currency = currency.ToUpperInvariant(),
                    unitPrice,
                    exchangeRate = rate,
                    quotedAt = date ?? DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
