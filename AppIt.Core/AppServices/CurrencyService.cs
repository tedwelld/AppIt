using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class CurrencyService : ICurrencyService
    {
        private readonly AppItDbContext _db;

        public CurrencyService(AppItDbContext db)
        {
            _db = db;
        }

        public async Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto dto)
        {
            var currency = new Currency
            {
                Name = dto.Name,
                Code = dto.Code,
                IsActive = dto.IsActive
            };

            _db.Currencies.Add(currency);
            await _db.SaveChangesAsync();

            return new CurrencyDto
            {
                Id = currency.Id,
                Name = currency.Name,
                Code = currency.Code,
                IsActive = currency.IsActive
            };
        }

        public async Task<CurrencyDto?> GetCurrencyByIdAsync(int id)
        {
            var currency = await _db.Currencies.FindAsync(id);
            if (currency == null) return null;

            return new CurrencyDto
            {
                Id = currency.Id,
                Name = currency.Name,
                Code = currency.Code,
                IsActive = currency.IsActive
            };
        }

        public async Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync()
        {
            return await _db.Currencies
                .AsNoTracking()
                .OrderBy(c => c.Code)
                .Select(c => new CurrencyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        public async Task<CurrencyDto?> UpdateCurrencyAsync(int id, UpdateCurrencyDto dto)
        {
            var currency = await _db.Currencies.FindAsync(id);
            if (currency == null) return null;

            currency.Name = dto.Name;
            currency.Code = dto.Code;
            currency.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            return new CurrencyDto
            {
                Id = currency.Id,
                Name = currency.Name,
                Code = currency.Code,
                IsActive = currency.IsActive
            };
        }

        public async Task<bool> DeleteCurrencyAsync(int id)
        {
            var currency = await _db.Currencies.FindAsync(id);
            if (currency == null) return false;

            _db.Currencies.Remove(currency);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
