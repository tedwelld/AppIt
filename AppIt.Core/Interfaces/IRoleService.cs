using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces
{
    public interface IRoleService
    {
        Task<ServiceResponse<RoleDto>> CreateAsync(CreateRoleDto dto);
        Task<ServiceResponse<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<List<RoleDto>>> GetAllAsync();
        Task<ServiceResponse<RoleDto>> GetByIdAsync(int id);
    }
}
