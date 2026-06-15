using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class PricingService : IPricingService
    {
        private readonly AppItDbContext _context;
        private readonly IExchangeRateService _exchangeRates;

        public PricingService(AppItDbContext context, IExchangeRateService exchangeRates)
        {
            _context = context;
            _exchangeRates = exchangeRates;
        }

        public async Task<decimal> ResolveUnitPriceAsync(string serviceType, int serviceId, string currency, DateTime? date = null, int? consultantId = null)
        {
            var type = ServicePriceService.NormalizeServiceType(serviceType);
            var targetCurrency = ServicePriceService.NormalizeCurrency(currency);
            var effectiveDate = (date ?? DateTime.UtcNow).Date;

            // 1) Special product price (date window; consultant-specific preferred over general).
            var special = await ResolveSpecialPriceAsync(type, serviceId, consultantId, effectiveDate);
            if (special != null)
            {
                return await ConvertAsync(special.Value.amount, special.Value.currency, targetCurrency, effectiveDate);
            }

            // 2) Active service price in the requested currency.
            var direct = await _context.ServicePrices
                .AsNoTracking()
                .Where(p => p.IsActive && p.ServiceType == type && p.ServiceId == serviceId && p.CurrencyCode == targetCurrency)
                .Select(p => (decimal?)p.UnitPrice)
                .FirstOrDefaultAsync();
            if (direct.HasValue)
            {
                return direct.Value;
            }

            // 3) Active USD service price, converted to the requested currency.
            if (targetCurrency != "USD")
            {
                var usdServicePrice = await _context.ServicePrices
                    .AsNoTracking()
                    .Where(p => p.IsActive && p.ServiceType == type && p.ServiceId == serviceId && p.CurrencyCode == "USD")
                    .Select(p => (decimal?)p.UnitPrice)
                    .FirstOrDefaultAsync();
                if (usdServicePrice.HasValue && usdServicePrice.Value > 0)
                {
                    return await ConvertAsync(usdServicePrice.Value, "USD", targetCurrency, effectiveDate);
                }
            }

            // 4) USD base price on the catalogue entity, converted to the requested currency.
            var baseUsd = await GetBasePriceUsdAsync(type, serviceId);
            if (baseUsd.HasValue && baseUsd.Value > 0)
            {
                return await ConvertAsync(baseUsd.Value, "USD", targetCurrency, effectiveDate);
            }

            throw new InvalidOperationException($"No active {targetCurrency} price is configured for this {type} service.");
        }

        public async Task<decimal> GetEffectiveRateAsync(string currency, DateTime? date = null)
        {
            var code = ServicePriceService.NormalizeCurrency(currency);
            if (code == "USD") return 1m;
            return await RequireRateAsync(code, date ?? DateTime.UtcNow);
        }

        private async Task<(decimal amount, string currency)?> ResolveSpecialPriceAsync(string type, int serviceId, int? consultantId, DateTime effectiveDate)
        {
            // ProductType is stored as a free string; compare case-insensitively (or treat null as a wildcard).
            var candidates = await _context.SpecialProductPrices
                .AsNoTracking()
                .Where(s => s.IsActive
                    && s.ProductId == serviceId
                    && s.StartDate.Date <= effectiveDate
                    && (s.EndDate == null || s.EndDate.Value.Date >= effectiveDate))
                .ToListAsync();

            var matching = candidates
                .Where(s => string.IsNullOrWhiteSpace(s.ProductType)
                    || string.Equals(ServicePriceService.NormalizeServiceType(s.ProductType), type, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matching.Count == 0) return null;

            // Prefer a consultant-specific special, otherwise the most recent general one.
            var pick = matching
                .Where(s => consultantId.HasValue && s.ConsultantId == consultantId.Value)
                .OrderByDescending(s => s.StartDate)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault()
                ?? matching
                    .Where(s => s.ConsultantId == null)
                    .OrderByDescending(s => s.StartDate)
                    .ThenByDescending(s => s.Id)
                    .FirstOrDefault();

            return pick == null ? null : (pick.SpecialPrice, ServicePriceService.NormalizeCurrency(pick.CurrencyCode));
        }

        private async Task<decimal?> GetBasePriceUsdAsync(string type, int serviceId)
        {
            return type switch
            {
                "Product" => await _context.Products
                    .Where(p => p.ProductId == serviceId && p.IsActive)
                    .Select(p => (decimal?)p.BasePriceUsd)
                    .FirstOrDefaultAsync(),
                "Accommodation" => await _context.Accommodations
                    .Where(a => a.Id == serviceId && a.IsActive)
                    .Select(a => (decimal?)a.BasePriceUsd)
                    .FirstOrDefaultAsync(),
                "Activity" => await _context.Activities
                    .Where(a => a.Id == serviceId && a.IsActive)
                    .Select(a => (decimal?)a.BasePriceUsd)
                    .FirstOrDefaultAsync(),
                "Transfer" => await _context.Transfers
                    .Where(t => t.Id == serviceId && t.IsActive)
                    .Select(t => (decimal?)t.BasePriceUsd)
                    .FirstOrDefaultAsync(),
                "Tour" => await _context.Tours
                    .Where(t => t.Id == serviceId && t.IsActive)
                    .Select(t => (decimal?)t.BasePriceUsd)
                    .FirstOrDefaultAsync(),
                _ => null
            };
        }

        private async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, DateTime date)
        {
            var from = ServicePriceService.NormalizeCurrency(fromCurrency);
            var to = ServicePriceService.NormalizeCurrency(toCurrency);
            if (from == to) return amount;

            var fromRate = await RequireRateAsync(from, date);
            var toRate = await RequireRateAsync(to, date);

            var usd = fromRate > 0 ? amount / fromRate : amount;
            return decimal.Round(usd * toRate, 2, MidpointRounding.AwayFromZero);
        }

        private async Task<decimal> RequireRateAsync(string currency, DateTime date)
        {
            if (currency == "USD") return 1m;
            var rate = await _exchangeRates.GetEffectiveRateAsync(currency, date);
            if (rate?.Rate is not > 0)
            {
                throw new InvalidOperationException($"No exchange rate is configured for {currency} on or before {date:yyyy-MM-dd}. Capture a rate under Cashier > Exchange Rates first.");
            }
            return rate!.Rate;
        }
    }
}
