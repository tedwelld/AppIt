using System.Collections.Generic;
using System.Threading.Tasks;
using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IComboService
    {
        Task<IEnumerable<ComboReadDto>> GetAllAsync();
        Task<ComboReadDto?> GetByIdAsync(int id);
        Task<ComboReadDto> CreateAsync(CreateComboDto dto);
        Task<ComboReadDto?> UpdateAsync(int id, UpdateComboDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ReservationServiceItemSplitDto>> ExpandComboSplitsAsync(int comboId, decimal headerTotal, int quantity);
    }
}
