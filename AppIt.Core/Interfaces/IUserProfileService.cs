using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IUserProfileService
    {
        Task<IEnumerable<UserProfileReadDto>> GetAllAsync();
        Task<UserProfileReadDto?> GetByIdAsync(int id);
        Task<UserProfileReadDto> CreateAsync(CreateUserProfileDto dto);
        Task<UserProfileReadDto?> UpdateAsync(UpdateUserProfileDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
