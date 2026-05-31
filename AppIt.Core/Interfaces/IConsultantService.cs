using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IConsultantService
    {
        Task<IEnumerable<ConsultantReadDto>> GetAllAsync();
        Task<ConsultantReadDto?> GetByIdAsync(int id);
        Task<ConsultantReadDto> CreateAsync(CreateConsultantDto dto);
        Task<ConsultantReadDto?> UpdateAsync(UpdateConsultantDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
