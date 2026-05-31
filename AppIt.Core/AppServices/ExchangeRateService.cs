using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly AppItDbContext _db;

        public ExchangeRateService(AppItDbContext db)
        {
            _db = db;
        }

        public async Task<ExchangeRateDto> CreateExchangeRateAsync(CreateExchangeRateDto dto)
        {
            var rate = new ExchangeRate
            {
                CurrencyCode = dto.CurrencyCode,
                Rate = dto.Rate,
                EffectiveDate = dto.EffectiveDate,
                CreatedDate = DateTime.UtcNow
            };

            _db.ExchangeRates.Add(rate);
            await _db.SaveChangesAsync();

            return new ExchangeRateDto
            {
                Id = rate.Id,
                CurrencyCode = rate.CurrencyCode,
                Rate = rate.Rate,
                EffectiveDate = rate.EffectiveDate,
                CreatedDate = rate.CreatedDate
            };
        }

        public async Task<ExchangeRateDto?> GetExchangeRateByIdAsync(int id)
        {
            var rate = await _db.ExchangeRates.FindAsync(id);
            if (rate == null) return null;

            return new ExchangeRateDto
            {
                Id = rate.Id,
                CurrencyCode = rate.CurrencyCode,
                Rate = rate.Rate,
                EffectiveDate = rate.EffectiveDate,
                CreatedDate = rate.CreatedDate
            };
        }

        public async Task<IEnumerable<ExchangeRateDto>> GetAllExchangeRatesAsync()
        {
            return await _db.ExchangeRates
                .AsNoTracking()
                .OrderByDescending(r => r.EffectiveDate)
                .ThenBy(r => r.CurrencyCode)
                .Select(r => new ExchangeRateDto
                {
                    Id = r.Id,
                    CurrencyCode = r.CurrencyCode,
                    Rate = r.Rate,
                    EffectiveDate = r.EffectiveDate,
                    CreatedDate = r.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesByDateAsync(DateTime date)
        {
            return await _db.ExchangeRates
                .AsNoTracking()
                .Where(r => r.EffectiveDate.Date == date.Date)
                .OrderBy(r => r.CurrencyCode)
                .Select(r => new ExchangeRateDto
                {
                    Id = r.Id,
                    CurrencyCode = r.CurrencyCode,
                    Rate = r.Rate,
                    EffectiveDate = r.EffectiveDate,
                    CreatedDate = r.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ExchangeRateDto>> GetEffectiveRatesAsync(DateTime date)
        {
            // A rate applies from its effective date forward. For each currency, pick the most
            // recent rate whose effective date is on/before the requested date.
            var candidates = await _db.ExchangeRates
                .AsNoTracking()
                .Where(r => r.EffectiveDate.Date <= date.Date)
                .ToListAsync();

            return candidates
                .GroupBy(r => r.CurrencyCode)
                .Select(group => group
                    .OrderByDescending(r => r.EffectiveDate)
                    .ThenByDescending(r => r.Id)
                    .First())
                .Select(r => new ExchangeRateDto
                {
                    Id = r.Id,
                    CurrencyCode = r.CurrencyCode,
                    Rate = r.Rate,
                    EffectiveDate = r.EffectiveDate,
                    CreatedDate = r.CreatedDate
                })
                .OrderBy(r => r.CurrencyCode)
                .ToList();
        }

        public async Task<ExchangeRateDto?> GetEffectiveRateAsync(string currencyCode, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return null;

            var code = currencyCode.Trim().ToUpperInvariant();
            if (code == "USD")
                return new ExchangeRateDto { CurrencyCode = "USD", Rate = 1m, EffectiveDate = date, CreatedDate = date };

            var rate = await _db.ExchangeRates
                .AsNoTracking()
                .Where(r => r.CurrencyCode == code && r.EffectiveDate.Date <= date.Date)
                .OrderByDescending(r => r.EffectiveDate)
                .ThenByDescending(r => r.Id)
                .FirstOrDefaultAsync();

            if (rate == null) return null;

            return new ExchangeRateDto
            {
                Id = rate.Id,
                CurrencyCode = rate.CurrencyCode,
                Rate = rate.Rate,
                EffectiveDate = rate.EffectiveDate,
                CreatedDate = rate.CreatedDate
            };
        }

        public async Task<ExchangeRateDto?> UpdateExchangeRateAsync(int id, UpdateExchangeRateDto dto)
        {
            var rate = await _db.ExchangeRates.FindAsync(id);
            if (rate == null) return null;

            rate.Rate = dto.Rate;
            rate.EffectiveDate = dto.EffectiveDate;

            await _db.SaveChangesAsync();

            return new ExchangeRateDto
            {
                Id = rate.Id,
                CurrencyCode = rate.CurrencyCode,
                Rate = rate.Rate,
                EffectiveDate = rate.EffectiveDate,
                CreatedDate = rate.CreatedDate
            };
        }

        public async Task<bool> DeleteExchangeRateAsync(int id)
        {
            var rate = await _db.ExchangeRates.FindAsync(id);
            if (rate == null) return false;

            _db.ExchangeRates.Remove(rate);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
