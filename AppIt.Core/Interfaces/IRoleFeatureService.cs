using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IRoleFeatureService
    {
        Task<ServiceResponse<List<RoleFeatureDto>>> GetRoleFeaturesAsync();
        Task<ServiceResponse<RoleFeatureDto>> CreateRoleFeatureAsync(RoleFeatureDto roleFeatureDto);
        Task<ServiceResponse<RoleFeatureDto>> UpdateRoleFeatureAsync(int roleId, int featureId, RoleFeatureDto roleFeatureDto);
        Task<ServiceResponse<bool>> DeleteRoleFeatureAsync(int roleId, int featureId);
        Task<ServiceResponse<List<RoleFeaturePermissionDto>>> GetRoleFeaturePermissionsAsync();
        Task<ServiceResponse<RoleFeaturePermissionDto>> CreateRoleFeaturePermissionAsync(RoleFeaturePermissionDto roleFeaturePermissionDto);
        Task<ServiceResponse<RoleFeaturePermissionDto>> UpdateRoleFeaturePermissionAsync(int roleId, int featureId, int permissionId, RoleFeaturePermissionDto roleFeaturePermissionDto);
        Task<ServiceResponse<bool>> DeleteRoleFeaturePermissionAsync(int roleId, int featureId, int permissionId);
    }
}
