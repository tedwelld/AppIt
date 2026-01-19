using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AppIt.Core.AppServices
{
    public class RoleService : IRoleService
    {
        public Task<ServiceResponse<RoleDto>> CreateRoleAsync(RoleDto roleDto)
        {
            return Task.FromResult(new ServiceResponse<RoleDto>(roleDto, "Role created (stub)"));
        }

        public Task<ServiceResponse<bool>> DeleteRoleAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<bool>(true, "Role deleted (stub)"));
        }

        public Task<ServiceResponse<RoleDto>> GetRoleByIdAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<RoleDto>(new RoleDto(), "Role retrieved (stub)"));
        }

        public Task<ServiceResponse<List<RoleDto>>> GetRolesAsync()
        {
            return Task.FromResult(new ServiceResponse<List<RoleDto>>(new List<RoleDto>(), "Roles retrieved (stub)"));
        }

        public Task<ServiceResponse<RoleDto>> UpdateRoleAsync(int id, RoleDto roleDto)
        {
            return Task.FromResult(new ServiceResponse<RoleDto>(roleDto, "Role updated (stub)"));
        }
    }
}
