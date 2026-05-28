using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces
{
    public interface ICurrencyService
    {
        Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto dto);
        Task<CurrencyDto?> GetCurrencyByIdAsync(int id);
        Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync();
        Task<CurrencyDto?> UpdateCurrencyAsync(int id, UpdateCurrencyDto dto);
        Task<bool> DeleteCurrencyAsync(int id);
    }
}
