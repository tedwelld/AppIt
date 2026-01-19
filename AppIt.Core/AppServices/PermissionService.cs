using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AppIt.Core.AppServices
{
    public class PermissionService : IPermissionService
    {
        public Task<ServiceResponse<CreatePermissionDto>> CreatePermissionAsync(CreatePermissionDto createDto)
        {
            return Task.FromResult(new ServiceResponse<CreatePermissionDto>(createDto, "Permission created (stub)"));
        }

        public Task<ServiceResponse<DeletePermissionDto>> DeletePermissionAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<DeletePermissionDto>(new DeletePermissionDto { Id = id }, "Permission deleted (stub)"));
        }

        public Task<ServiceResponse<GetPermissionDto>> GetPermissionByIdAsync(int id)
        {
            return Task.FromResult(new ServiceResponse<GetPermissionDto>(new GetPermissionDto { Id = id, Name = "Permission" }, "Permission retrieved (stub)"));
        }

        public Task<ServiceResponse<PermissionDto>> GetPermissionAsync()
        {
            return Task.FromResult(new ServiceResponse<PermissionDto>(new PermissionDto(), "Permissions retrieved (stub)"));
        }

        public Task<ServiceResponse<UpdatePermissionDto>> UpdatePermissionAsync(int id, UpdatePermissionDto updateDto)
        {
            return Task.FromResult(new ServiceResponse<UpdatePermissionDto>(updateDto, "Permission updated (stub)"));
        }
    }
}
