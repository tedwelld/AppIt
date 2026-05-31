using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
