using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IFeatureService
    {
        Task<FeatureReadDto> CreateAsync(CreateFeatureDto dto);
        Task<FeatureReadDto?> UpdateAsync(UpdateFeatureDto dto);
        Task<bool> DeleteAsync(int id);
        Task<FeatureReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<FeatureReadDto>> GetAllAsync();
    }
}
