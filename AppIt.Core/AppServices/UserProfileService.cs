using AppIt.Core.DTOs.AppIt.Core.DTOs.AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

public class UserProfileService : IUserProfileService
{
    private readonly AppItDbContext _context;

    public UserProfileService(AppItDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileReadDto> CreateAsync(
        int accountId,
        CreateUserProfileDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == accountId, cancellationToken);

        if (!accountExists)
            throw new InvalidOperationException("Account does not exist.");

        var profile = new UserProfile
        {
           
            DisplayName = dto.DisplayName?.Trim()
        };

        _context.UserProfiles.Add(profile);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("User profile already exists for this account.");
        }

        return new UserProfileReadDto(
            profile.Id,
            profile.DisplayName,
            profile.IsActive
        );
    }

    public Task<AppIt.Core.DTOs.AppIt.Core.DTOs.UserProfileReadDto> CreateAsync(CreateUserProfileDto dto)
    {
        throw new NotImplementedException();
    }
}
