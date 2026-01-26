using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class FeaturePermissionService : IFeaturePermissionService
    {
        private readonly AppItDbContext _context;

        public FeaturePermissionService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<FeaturePermissionReadDto> CreateAsync(CreateFeaturePermissionDto dto)
        {
            var feature = await _context.Features.FindAsync(dto.FeatureId);
            var permission = await _context.Permissions.FindAsync(dto.PermissionId);

            if (feature == null || permission == null)
                throw new ArgumentException("Feature or Permission not found");

            var fp = new FeaturePermission
            {
                FeatureId = dto.FeatureId,
                PermissionId = dto.PermissionId,
                Feature = feature,
                Permission = permission
            };

            _context.FeaturePermissions.Add(fp);
            await _context.SaveChangesAsync();

            return ToReadDto(fp);
        }

        public async Task<FeaturePermissionReadDto?> UpdateAsync(UpdateFeaturePermissionDto dto)
        {
            var fp = await _context.FeaturePermissions.FindAsync(dto.FeaturePermissionId);
            if (fp == null) return null;

            var feature = await _context.Features.FindAsync(dto.FeatureId);
            var permission = await _context.Permissions.FindAsync(dto.PermissionId);

            if (feature == null || permission == null)
                throw new ArgumentException("Feature or Permission not found");

            fp.FeatureId = dto.FeatureId;
            fp.PermissionId = dto.PermissionId;
            fp.Feature = feature;
            fp.Permission = permission;

            await _context.SaveChangesAsync();
            return ToReadDto(fp);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var fp = await _context.FeaturePermissions.FindAsync(id);
            if (fp == null) return false;

            _context.FeaturePermissions.Remove(fp);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FeaturePermissionReadDto?> GetByIdAsync(int id)
        {
            var fp = await _context.FeaturePermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FeaturePermissionId == id);

            return fp == null ? null : ToReadDto(fp);
        }

        public async Task<IEnumerable<FeaturePermissionReadDto>> GetAllAsync()
        {
            var fps = await _context.FeaturePermissions
                .AsNoTracking()
                .ToListAsync();

            return fps.Select(ToReadDto);
        }

        private FeaturePermissionReadDto ToReadDto(FeaturePermission fp) => new()
        {
            FeaturePermissionId = fp.FeaturePermissionId,
            FeatureId = fp.FeatureId,
            PermissionId = fp.PermissionId
        };
    }
}
