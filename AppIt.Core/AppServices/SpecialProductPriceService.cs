using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Core.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class SpecialProductPriceService : ISpecialProductPriceService
    {
        private readonly AppItDbContext _context;

        public SpecialProductPriceService(AppItDbContext context) => _context = context;

        private static SpecialProductPriceReadDto Map(SpecialProductPrice s) => new()
        {
            Id = s.Id, ProductId = s.ProductId, ProductType = s.ProductType, ConsultantId = s.ConsultantId,
            SpecialPrice = s.SpecialPrice, CurrencyCode = s.CurrencyCode, StartDate = s.StartDate,
            EndDate = s.EndDate, Notes = s.Notes, IsActive = s.IsActive, CreatedAt = s.CreatedAt
        };

        public async Task<IEnumerable<SpecialProductPriceReadDto>> GetAllAsync() =>
            await _context.SpecialProductPrices.AsNoTracking().OrderByDescending(s => s.StartDate)
                .Select(s => new SpecialProductPriceReadDto
                {
                    Id = s.Id, ProductId = s.ProductId, ProductType = s.ProductType, ConsultantId = s.ConsultantId,
                    SpecialPrice = s.SpecialPrice, CurrencyCode = s.CurrencyCode, StartDate = s.StartDate,
                    EndDate = s.EndDate, Notes = s.Notes, IsActive = s.IsActive, CreatedAt = s.CreatedAt
                }).ToListAsync();

        public async Task<SpecialProductPriceReadDto?> GetByIdAsync(int id)
        {
            var s = await _context.SpecialProductPrices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return s == null ? null : Map(s);
        }

        public async Task<IEnumerable<SpecialProductPriceReadDto>> GetByProductAsync(int productId) =>
            await _context.SpecialProductPrices.AsNoTracking().Where(s => s.ProductId == productId)
                .Select(s => new SpecialProductPriceReadDto
                {
                    Id = s.Id, ProductId = s.ProductId, ProductType = s.ProductType, ConsultantId = s.ConsultantId,
                    SpecialPrice = s.SpecialPrice, CurrencyCode = s.CurrencyCode, StartDate = s.StartDate,
                    EndDate = s.EndDate, Notes = s.Notes, IsActive = s.IsActive, CreatedAt = s.CreatedAt
                }).ToListAsync();

        public async Task<SpecialProductPriceReadDto> CreateAsync(CreateSpecialProductPriceDto dto)
        {
            await ValidateAsync(dto.ProductId, dto.ProductType, dto.StartDate, dto.EndDate, dto.SpecialPrice, null);
            var entity = new SpecialProductPrice
            {
                ProductId = dto.ProductId, ProductType = dto.ProductType, ConsultantId = dto.ConsultantId,
                SpecialPrice = dto.SpecialPrice, CurrencyCode = dto.CurrencyCode, StartDate = dto.StartDate,
                EndDate = dto.EndDate, Notes = dto.Notes, IsActive = dto.IsActive
            };
            _context.SpecialProductPrices.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<SpecialProductPriceReadDto?> UpdateAsync(UpdateSpecialProductPriceDto dto)
        {
            var entity = await _context.SpecialProductPrices.FindAsync(dto.Id);
            if (entity == null) return null;
            await ValidateAsync(dto.ProductId, dto.ProductType, dto.StartDate, dto.EndDate, dto.SpecialPrice, dto.Id);
            entity.ProductId = dto.ProductId; entity.ProductType = dto.ProductType;
            entity.ConsultantId = dto.ConsultantId; entity.SpecialPrice = dto.SpecialPrice;
            entity.CurrencyCode = dto.CurrencyCode; entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate; entity.Notes = dto.Notes; entity.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.SpecialProductPrices.FindAsync(id);
            if (entity == null) return false;
            _context.SpecialProductPrices.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task ValidateAsync(int? productId, string? productType, DateTime startDate, DateTime? endDate, decimal specialPrice, int? currentId)
        {
            if (!productId.HasValue || productId.Value <= 0)
            {
                throw new InvalidOperationException("Select a catalog service for this special price.");
            }

            if (specialPrice <= 0)
            {
                throw new InvalidOperationException("Special price must be greater than zero.");
            }

            var normalizedType = ServicePriceService.NormalizeServiceType(productType ?? string.Empty);
            if (!await ServiceExistsAsync(normalizedType, productId.Value))
            {
                throw new InvalidOperationException($"The selected {normalizedType} (#{productId}) does not exist or is inactive.");
            }

            if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            {
                throw new InvalidOperationException("End date must be on or after the start date.");
            }
        }

        private async Task<bool> ServiceExistsAsync(string serviceType, int serviceId)
        {
            return serviceType switch
            {
                "Product" => await _context.Products.AnyAsync(p => p.ProductId == serviceId && p.IsActive),
                "Accommodation" => await _context.Accommodations.AnyAsync(a => a.Id == serviceId && a.IsActive),
                "Activity" => await _context.Activities.AnyAsync(a => a.Id == serviceId && a.IsActive),
                "Transfer" => await _context.Transfers.AnyAsync(t => t.Id == serviceId && t.IsActive),
                "Tour" => await _context.Tours.AnyAsync(t => t.Id == serviceId && t.IsActive),
                _ => false
            };
        }

        public async Task<SpecialProductPriceReadDto?> VerifyAsync(int id)
        {
            var e = await _context.SpecialProductPrices.FindAsync(id);
            if (e == null) return null;
            e.IsVerified = true;
            e.VerifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(e);
        }

        public async Task<SpecialProductPriceReadDto?> ApproveAsync(int id)
        {
            var e = await _context.SpecialProductPrices.FindAsync(id);
            if (e == null) return null;
            e.IsApproved = true;
            e.ApprovedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(e);
        }

        public async Task<SpecialProductPriceReadDto?> SendToAgentAsync(int id)
        {
            var e = await _context.SpecialProductPrices.FindAsync(id);
            if (e == null) return null;
            e.ApprovalKey = Guid.NewGuid().ToString("N");
            e.Sent = true;
            await _context.SaveChangesAsync();
            return Map(e);
        }

        public async Task<SpecialProductPriceReadDto?> AgentApprovalAsync(string approvalKey)
        {
            var e = await _context.SpecialProductPrices.FirstOrDefaultAsync(s => s.ApprovalKey == approvalKey);
            if (e == null) return null;
            e.IsAgentApproved = true;
            await _context.SaveChangesAsync();
            return Map(e);
        }
    }
}
