using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AppIt.Core.AppServices
{
    public class RoleFeatureService : IRoleFeatureService
    {
        public Task<ServiceResponse<RoleFeatureDto>> CreateRoleFeatureAsync(RoleFeatureDto roleFeatureDto)
        {
            return Task.FromResult(new ServiceResponse<RoleFeatureDto>(roleFeatureDto, "RoleFeature created (stub)"));
        }

        public Task<ServiceResponse<bool>> DeleteRoleFeatureAsync(int roleId, int featureId)
        {
            return Task.FromResult(new ServiceResponse<bool>(true, "RoleFeature deleted (stub)"));
        }

        public Task<ServiceResponse<List<RoleFeatureDto>>> GetRoleFeaturesAsync()
        {
            return Task.FromResult(new ServiceResponse<List<RoleFeatureDto>>(new List<RoleFeatureDto>(), "RoleFeatures retrieved (stub)"));
        }

        public Task<ServiceResponse<RoleFeatureDto>> UpdateRoleFeatureAsync(int roleId, int featureId, RoleFeatureDto roleFeatureDto)
        {
            return Task.FromResult(new ServiceResponse<RoleFeatureDto>(roleFeatureDto, "RoleFeature updated (stub)"));
        }

        public Task<ServiceResponse<List<RoleFeaturePermissionDto>>> GetRoleFeaturePermissionsAsync()
        {
            return Task.FromResult(new ServiceResponse<List<RoleFeaturePermissionDto>>(new List<RoleFeaturePermissionDto>(), "RoleFeaturePermissions retrieved (stub)"));
        }

        public Task<ServiceResponse<RoleFeaturePermissionDto>> CreateRoleFeaturePermissionAsync(RoleFeaturePermissionDto roleFeaturePermissionDto)
        {
            return Task.FromResult(new ServiceResponse<RoleFeaturePermissionDto>(roleFeaturePermissionDto, "RoleFeaturePermission created (stub)"));
        }

        public Task<ServiceResponse<RoleFeaturePermissionDto>> UpdateRoleFeaturePermissionAsync(int roleId, int featureId, int permissionId, RoleFeaturePermissionDto roleFeaturePermissionDto)
        {
            return Task.FromResult(new ServiceResponse<RoleFeaturePermissionDto>(roleFeaturePermissionDto, "RoleFeaturePermission updated (stub)"));
        }

        public Task<ServiceResponse<bool>> DeleteRoleFeaturePermissionAsync(int roleId, int featureId, int permissionId)
        {
            return Task.FromResult(new ServiceResponse<bool>(true, "RoleFeaturePermission deleted (stub)"));
        }
    }
}
