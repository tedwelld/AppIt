using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IPermissionService
    {
        Task<ServiceResponse<PermissionDto>> GetPermissionAsync();
        Task<ServiceResponse<CreatePermissionDto>> CreatePermissionAsync(CreatePermissionDto createDto);
        Task<ServiceResponse<UpdatePermissionDto>> UpdatePermissionAsync(int id, UpdatePermissionDto updateDto);
        Task<ServiceResponse<DeletePermissionDto>> DeletePermissionAsync(int id);
        Task<ServiceResponse<GetPermissionDto>> GetPermissionByIdAsync(int id);
    }
}
