using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppItDbContext _context;

        public UserProfileService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserProfileReadDto>> GetAllAsync()
        {
            return await _context.UserProfiles
                .AsNoTracking()
                .Select(p => new UserProfileReadDto(p.Id, p.DisplayName, p.IsActive))
                .ToListAsync();
        }

        public async Task<UserProfileReadDto?> GetByIdAsync(int id)
        {
            var p = await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return p == null ? null : new UserProfileReadDto(p.Id, p.DisplayName, p.IsActive);
        }

        public async Task<UserProfileReadDto> CreateAsync(CreateUserProfileDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var displayName = dto.DisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name is required.", nameof(dto));

            var profile = new UserProfile { DisplayName = displayName };
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return new UserProfileReadDto(profile.Id, profile.DisplayName, profile.IsActive);
        }

        public async Task<UserProfileReadDto?> UpdateAsync(UpdateUserProfileDto dto)
        {
            var profile = await _context.UserProfiles.FindAsync(dto.Id);
            if (profile == null) return null;

            profile.DisplayName = dto.DisplayName?.Trim() ?? profile.DisplayName;
            profile.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            return new UserProfileReadDto(profile.Id, profile.DisplayName, profile.IsActive);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var profile = await _context.UserProfiles.FindAsync(id);
            if (profile == null) return false;

            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
