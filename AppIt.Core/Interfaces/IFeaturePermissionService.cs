using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IFeaturePermissionService
    {
        Task<FeaturePermissionReadDto> CreateAsync(CreateFeaturePermissionDto dto);
        Task<FeaturePermissionReadDto?> UpdateAsync(UpdateFeaturePermissionDto dto);
        Task<bool> DeleteAsync(int id);
        Task<FeaturePermissionReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<FeaturePermissionReadDto>> GetAllAsync();
    }
}
