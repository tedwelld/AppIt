using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class ServicePriceService : IServicePriceService
    {
        private readonly AppItDbContext _context;

        public ServicePriceService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServicePriceReadDto>> GetAllAsync()
        {
            return await _context.ServicePrices
                .AsNoTracking()
                .OrderBy(p => p.ServiceType)
                .ThenBy(p => p.ServiceId)
                .ThenBy(p => p.CurrencyCode)
                .Select(p => new ServicePriceReadDto
                {
                    Id = p.Id,
                    ServiceType = p.ServiceType,
                    ServiceId = p.ServiceId,
                    CurrencyCode = p.CurrencyCode,
                    UnitPrice = p.UnitPrice,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ServicePriceReadDto?> GetByIdAsync(int id)
        {
            var price = await _context.ServicePrices.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return price == null ? null : ServicePricingMapper.ToReadDto(price);
        }

        public async Task<IEnumerable<ServicePriceReadDto>> GetByServiceAsync(string serviceType, int serviceId)
        {
            var normalizedType = NormalizeServiceType(serviceType);
            return await _context.ServicePrices
                .AsNoTracking()
                .Where(p => p.ServiceType == normalizedType && p.ServiceId == serviceId)
                .OrderBy(p => p.CurrencyCode)
                .Select(p => new ServicePriceReadDto
                {
                    Id = p.Id,
                    ServiceType = p.ServiceType,
                    ServiceId = p.ServiceId,
                    CurrencyCode = p.CurrencyCode,
                    UnitPrice = p.UnitPrice,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ServicePriceReadDto> CreateAsync(CreateServicePriceDto dto)
        {
            var serviceType = NormalizeServiceType(dto.ServiceType);
            var currencyCode = NormalizeCurrency(dto.CurrencyCode);
            await EnsureServiceExistsAsync(serviceType, dto.ServiceId);
            await EnsureNoActiveDuplicateAsync(serviceType, dto.ServiceId, currencyCode, null, dto.IsActive);

            var price = new ServicePrice
            {
                ServiceType = serviceType,
                ServiceId = dto.ServiceId,
                CurrencyCode = currencyCode,
                UnitPrice = dto.UnitPrice,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServicePrices.Add(price);
            await _context.SaveChangesAsync();
            return ServicePricingMapper.ToReadDto(price);
        }

        public async Task<ServicePriceReadDto?> UpdateAsync(UpdateServicePriceDto dto)
        {
            var price = await _context.ServicePrices.FindAsync(dto.Id);
            if (price == null) return null;

            var serviceType = NormalizeServiceType(dto.ServiceType);
            var currencyCode = NormalizeCurrency(dto.CurrencyCode);
            await EnsureServiceExistsAsync(serviceType, dto.ServiceId);
            await EnsureNoActiveDuplicateAsync(serviceType, dto.ServiceId, currencyCode, dto.Id, dto.IsActive);

            price.ServiceType = serviceType;
            price.ServiceId = dto.ServiceId;
            price.CurrencyCode = currencyCode;
            price.UnitPrice = dto.UnitPrice;
            price.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return ServicePricingMapper.ToReadDto(price);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var price = await _context.ServicePrices.FindAsync(id);
            if (price == null) return false;

            _context.ServicePrices.Remove(price);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task EnsureNoActiveDuplicateAsync(string serviceType, int serviceId, string currencyCode, int? currentId, bool isActive)
        {
            if (!isActive) return;

            var exists = await _context.ServicePrices.AnyAsync(p =>
                p.IsActive
                && p.ServiceType == serviceType
                && p.ServiceId == serviceId
                && p.CurrencyCode == currencyCode
                && (!currentId.HasValue || p.Id != currentId.Value));

            if (exists)
            {
                throw new InvalidOperationException("An active price already exists for this service and currency.");
            }
        }

        private async Task EnsureServiceExistsAsync(string serviceType, int serviceId)
        {
            var exists = serviceType switch
            {
                "Product" => await _context.Products.AnyAsync(p => p.ProductId == serviceId),
                "Accommodation" => await _context.Accommodations.AnyAsync(a => a.Id == serviceId),
                "Activity" => await _context.Activities.AnyAsync(a => a.Id == serviceId),
                "Transfer" => await _context.Transfers.AnyAsync(t => t.Id == serviceId),
                "Tour" => await _context.Tours.AnyAsync(t => t.Id == serviceId),
                _ => false
            };

            if (!exists)
            {
                throw new InvalidOperationException("The selected service was not found.");
            }
        }

        public static string NormalizeServiceType(string? serviceType)
        {
            var value = (serviceType ?? string.Empty).Trim();
            return value.Equals("Accommodation", StringComparison.OrdinalIgnoreCase) ? "Accommodation"
                : value.Equals("Activity", StringComparison.OrdinalIgnoreCase) ? "Activity"
                : value.Equals("Transfer", StringComparison.OrdinalIgnoreCase) ? "Transfer"
                : value.Equals("Tour", StringComparison.OrdinalIgnoreCase) ? "Tour"
                : "Product";
        }

        public static string NormalizeCurrency(string? currencyCode)
        {
            return string.IsNullOrWhiteSpace(currencyCode) ? "USD" : currencyCode.Trim().ToUpperInvariant();
        }
    }
}
