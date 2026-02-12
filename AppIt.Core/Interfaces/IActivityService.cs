using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IActivityService
    {
        Task<ActivityReadDto> CreateAsync(CreateActivityDto dto);
        Task<ActivityReadDto?> UpdateAsync(UpdateActivityDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ActivityReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<ActivityReadDto>> GetAllAsync();
    }
}
