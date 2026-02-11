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

        public async Task<UserProfileReadDto> CreateAsync(CreateUserProfileDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var displayName = dto.DisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Display name is required.", nameof(dto));
            }

            var profile = new UserProfile
            {
                DisplayName = displayName
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return new UserProfileReadDto(profile.Id, profile.DisplayName, profile.IsActive);
        }
    }
}
