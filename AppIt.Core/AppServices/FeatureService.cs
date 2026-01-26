using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;


namespace AppIt.Core.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly AppItDbContext _context;

        public FeatureService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<FeatureReadDto> CreateAsync(CreateFeatureDto dto)
        {
            var feature = new Feature
            {
                Name = dto.Name,
                Description = dto.Description,
                Permission = dto.PermissionId.HasValue
                    ? await _context.Permissions.FindAsync(dto.PermissionId.Value)
                    : null
            };

            _context.Features.Add(feature);
            await _context.SaveChangesAsync();

            return ToReadDto(feature);
        }

        public async Task<FeatureReadDto?> UpdateAsync(UpdateFeatureDto dto)
        {
            var feature = await _context.Features.FindAsync(dto.Id);
            if (feature == null) return null;

            feature.Name = dto.Name;
            feature.Description = dto.Description;

            if (dto.PermissionId.HasValue)
            {
                feature.Permission = await _context.Permissions.FindAsync(dto.PermissionId.Value);
            }
            else
            {
                feature.Permission = null;
            }

            await _context.SaveChangesAsync();
            return ToReadDto(feature);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature == null) return false;

            _context.Features.Remove(feature);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FeatureReadDto?> GetByIdAsync(int id)
        {
            var feature = await _context.Features
                .Include(f => f.Permission)
                .FirstOrDefaultAsync(f => f.Id == id);

            return feature == null ? null : ToReadDto(feature);
        }

        public async Task<IEnumerable<FeatureReadDto>> GetAllAsync()
        {
            var features = await _context.Features
                .Include(f => f.Permission)
                .ToListAsync();

            return features.Select(ToReadDto);
        }

        private FeatureReadDto ToReadDto(Feature f) => new()
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
        
        };
    }
}
