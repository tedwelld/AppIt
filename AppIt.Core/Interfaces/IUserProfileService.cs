using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileReadDto> CreateAsync(CreateUserProfileDto dto);
    }
}
