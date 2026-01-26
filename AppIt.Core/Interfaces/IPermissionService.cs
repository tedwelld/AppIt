using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IPermissionService
    {
        Task<PermissionReadDto> CreateAsync(CreatePermissionDto dto);
        Task<PermissionReadDto?> UpdateAsync(UpdatePermissionDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PermissionReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<PermissionReadDto>> GetAllAsync();
    }
}
