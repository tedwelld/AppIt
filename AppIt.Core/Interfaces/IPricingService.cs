using System;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    /// <summary>
    /// Resolves the authoritative unit price for a catalogue service in a given currency.
    /// Used by booking checkout and ad-hoc service-item additions so pricing is never
    /// taken from client input.
    /// </summary>
    public interface IPricingService
    {
        /// <summary>
        /// Resolves the unit price for a service in the requested currency, following the
        /// priority chain: SpecialProductPrice -> ServicePrice -> FX-converted USD price/base.
        /// Throws <see cref="InvalidOperationException"/> when no rate can be resolved.
        /// </summary>
        Task<decimal> ResolveUnitPriceAsync(string serviceType, int serviceId, string currency, DateTime? date = null, int? consultantId = null);

        /// <summary>
        /// Returns the effective exchange rate (foreign units per 1 USD) for a currency on a date.
        /// Throws when a non-USD rate is not configured.
        /// </summary>
        Task<decimal> GetEffectiveRateAsync(string currency, DateTime? date = null);
    }
}
