using AppIt.Core.DTOs;
using AppIt.Data.EntityModels;

namespace AppIt.Core.Services
{
    internal static class ServicePricingMapper
    {
        public static ServicePriceReadDto ToReadDto(ServicePrice price) => new()
        {
            Id = price.Id,
            ServiceType = price.ServiceType,
            ServiceId = price.ServiceId,
            CurrencyCode = price.CurrencyCode,
            UnitPrice = price.UnitPrice,
            IsActive = price.IsActive,
            CreatedAt = price.CreatedAt
        };
    }
}
