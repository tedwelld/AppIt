using AppIt.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateDto> CreateExchangeRateAsync(CreateExchangeRateDto dto);
        Task<ExchangeRateDto?> GetExchangeRateByIdAsync(int id);
        Task<IEnumerable<ExchangeRateDto>> GetAllExchangeRatesAsync();
        Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesByDateAsync(DateTime date);
        Task<IEnumerable<ExchangeRateDto>> GetEffectiveRatesAsync(DateTime date);
        Task<ExchangeRateDto?> GetEffectiveRateAsync(string currencyCode, DateTime date);
        Task<ExchangeRateDto?> UpdateExchangeRateAsync(int id, UpdateExchangeRateDto dto);
        Task<bool> DeleteExchangeRateAsync(int id);
    }
}
