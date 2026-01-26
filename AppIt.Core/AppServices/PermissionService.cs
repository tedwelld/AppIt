using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppItDbContext _context;

        public PermissionService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<PermissionReadDto> CreateAsync(CreatePermissionDto dto)
        {
            var permission = new Permission
            {
                Name = dto.Name
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return ToReadDto(permission);
        }

        public async Task<PermissionReadDto?> UpdateAsync(UpdatePermissionDto dto)
        {
            var permission = await _context.Permissions.FindAsync(dto.PermissionId);
            if (permission == null) return null;

            permission.Name = dto.Name;

            await _context.SaveChangesAsync();
            return ToReadDto(permission);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return false;

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PermissionReadDto?> GetByIdAsync(int id)
        {
            var permission = await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PermissionId == id);
            return permission == null ? null : ToReadDto(permission);
        }

        public async Task<IEnumerable<PermissionReadDto>> GetAllAsync()
        {
            var permissions = await _context.Permissions
                .AsNoTracking()
                .ToListAsync();
            return permissions.Select(ToReadDto);
        }

        private PermissionReadDto ToReadDto(Permission p) => new()
        {
            PermissionId = p.PermissionId,
            Name = p.Name
        };
    }
}
