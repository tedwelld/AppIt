using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class RoleFeatureService : IRoleFeatureService
    {
        private readonly AppItDbContext _db;

        public RoleFeatureService(AppItDbContext db)
        {
            _db = db;
        }

        public async Task<ServiceResponse<List<RoleFeatureDto>>> GetRoleFeaturesAsync()
        {
            var items = await _db.RoleFeatures
                .AsNoTracking()
                .Select(rf => new RoleFeatureDto
                {
                    RoleId = rf.RoleId,
                    FeatureId = rf.FeatureId,
                    IsActivated = rf.IsActivated
                })
                .ToListAsync();

            return new ServiceResponse<List<RoleFeatureDto>>(items, "RoleFeatures retrieved");
        }

        public async Task<ServiceResponse<RoleFeatureDto>> CreateRoleFeatureAsync(RoleFeatureDto roleFeatureDto)
        {
            var roleExists = await _db.Roles.AnyAsync(r => r.RoleId == roleFeatureDto.RoleId);
            var featureExists = await _db.Features.AnyAsync(f => f.Id == roleFeatureDto.FeatureId);
            if (!roleExists || !featureExists)
            {
                return new ServiceResponse<RoleFeatureDto>(null, "Role or Feature not found") { Success = false };
            }

            var exists = await _db.RoleFeatures.AnyAsync(rf =>
                rf.RoleId == roleFeatureDto.RoleId && rf.FeatureId == roleFeatureDto.FeatureId);
            if (exists)
            {
                return new ServiceResponse<RoleFeatureDto>(null, "RoleFeature already exists") { Success = false };
            }

            var entity = new RoleFeature
            {
                RoleId = roleFeatureDto.RoleId,
                FeatureId = roleFeatureDto.FeatureId,
                IsActivated = roleFeatureDto.IsActivated
            };

            _db.RoleFeatures.Add(entity);
            await _db.SaveChangesAsync();

            return new ServiceResponse<RoleFeatureDto>(roleFeatureDto, "RoleFeature created");
        }

        public async Task<ServiceResponse<RoleFeatureDto>> UpdateRoleFeatureAsync(int roleId, int featureId, RoleFeatureDto roleFeatureDto)
        {
            var entity = await _db.RoleFeatures
                .FirstOrDefaultAsync(rf => rf.RoleId == roleId && rf.FeatureId == featureId);
            if (entity == null)
            {
                return new ServiceResponse<RoleFeatureDto>(null, "RoleFeature not found") { Success = false };
            }

            entity.IsActivated = roleFeatureDto.IsActivated;
            await _db.SaveChangesAsync();

            roleFeatureDto.RoleId = entity.RoleId;
            roleFeatureDto.FeatureId = entity.FeatureId;
            return new ServiceResponse<RoleFeatureDto>(roleFeatureDto, "RoleFeature updated");
        }

        public async Task<ServiceResponse<bool>> DeleteRoleFeatureAsync(int roleId, int featureId)
        {
            var entity = await _db.RoleFeatures
                .FirstOrDefaultAsync(rf => rf.RoleId == roleId && rf.FeatureId == featureId);
            if (entity == null)
            {
                return new ServiceResponse<bool>(false, "RoleFeature not found") { Success = false };
            }

            _db.RoleFeatures.Remove(entity);
            await _db.SaveChangesAsync();

            return new ServiceResponse<bool>(true, "RoleFeature deleted");
        }

        public async Task<ServiceResponse<List<RoleFeaturePermissionDto>>> GetRoleFeaturePermissionsAsync()
        {
            var items = await _db.RoleFeaturePermissions
                .AsNoTracking()
                .Select(rfp => new RoleFeaturePermissionDto
                {
                    RoleId = rfp.RoleId,
                    FeatureId = rfp.FeatureId,
                    PermissionId = rfp.PermissionId,
                    IsActivated = rfp.IsActivated
                })
                .ToListAsync();

            return new ServiceResponse<List<RoleFeaturePermissionDto>>(items, "RoleFeaturePermissions retrieved");
        }

        public async Task<ServiceResponse<RoleFeaturePermissionDto>> CreateRoleFeaturePermissionAsync(RoleFeaturePermissionDto roleFeaturePermissionDto)
        {
            var roleExists = await _db.Roles.AnyAsync(r => r.RoleId == roleFeaturePermissionDto.RoleId);
            var featureExists = await _db.Features.AnyAsync(f => f.Id == roleFeaturePermissionDto.FeatureId);
            var permissionExists = await _db.Permissions.AnyAsync(p => p.PermissionId == roleFeaturePermissionDto.PermissionId);
            if (!roleExists || !featureExists || !permissionExists)
            {
                return new ServiceResponse<RoleFeaturePermissionDto>(null, "Role, Feature, or Permission not found") { Success = false };
            }

            var exists = await _db.RoleFeaturePermissions.AnyAsync(rfp =>
                rfp.RoleId == roleFeaturePermissionDto.RoleId &&
                rfp.FeatureId == roleFeaturePermissionDto.FeatureId &&
                rfp.PermissionId == roleFeaturePermissionDto.PermissionId);
            if (exists)
            {
                return new ServiceResponse<RoleFeaturePermissionDto>(null, "RoleFeaturePermission already exists") { Success = false };
            }

            var entity = new RoleFeaturePermission
            {
                RoleId = roleFeaturePermissionDto.RoleId,
                FeatureId = roleFeaturePermissionDto.FeatureId,
                PermissionId = roleFeaturePermissionDto.PermissionId,
                IsActivated = roleFeaturePermissionDto.IsActivated
            };

            _db.RoleFeaturePermissions.Add(entity);
            await _db.SaveChangesAsync();

            return new ServiceResponse<RoleFeaturePermissionDto>(roleFeaturePermissionDto, "RoleFeaturePermission created");
        }

        public async Task<ServiceResponse<RoleFeaturePermissionDto>> UpdateRoleFeaturePermissionAsync(int roleId, int featureId, int permissionId, RoleFeaturePermissionDto roleFeaturePermissionDto)
        {
            var entity = await _db.RoleFeaturePermissions
                .FirstOrDefaultAsync(rfp =>
                    rfp.RoleId == roleId &&
                    rfp.FeatureId == featureId &&
                    rfp.PermissionId == permissionId);
            if (entity == null)
            {
                return new ServiceResponse<RoleFeaturePermissionDto>(null, "RoleFeaturePermission not found") { Success = false };
            }

            entity.IsActivated = roleFeaturePermissionDto.IsActivated;
            await _db.SaveChangesAsync();

            roleFeaturePermissionDto.RoleId = entity.RoleId;
            roleFeaturePermissionDto.FeatureId = entity.FeatureId;
            roleFeaturePermissionDto.PermissionId = entity.PermissionId;
            return new ServiceResponse<RoleFeaturePermissionDto>(roleFeaturePermissionDto, "RoleFeaturePermission updated");
        }

        public async Task<ServiceResponse<bool>> DeleteRoleFeaturePermissionAsync(int roleId, int featureId, int permissionId)
        {
            var entity = await _db.RoleFeaturePermissions
                .FirstOrDefaultAsync(rfp =>
                    rfp.RoleId == roleId &&
                    rfp.FeatureId == featureId &&
                    rfp.PermissionId == permissionId);
            if (entity == null)
            {
                return new ServiceResponse<bool>(false, "RoleFeaturePermission not found") { Success = false };
            }

            _db.RoleFeaturePermissions.Remove(entity);
            await _db.SaveChangesAsync();

            return new ServiceResponse<bool>(true, "RoleFeaturePermission deleted");
        }
    }
}
