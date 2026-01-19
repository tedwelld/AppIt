using System.Collections.Generic;
    using System.Threading.Tasks;
using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IRoleService
    {
        Task<ServiceResponse<List<RoleDto>>> GetRolesAsync();
        Task<ServiceResponse<RoleDto>> CreateRoleAsync(RoleDto roleDto);
        Task<ServiceResponse<RoleDto>> UpdateRoleAsync(int id, RoleDto roleDto);
        Task<ServiceResponse<bool>> DeleteRoleAsync(int id);
        Task<ServiceResponse<RoleDto>> GetRoleByIdAsync(int id);
    }
}
