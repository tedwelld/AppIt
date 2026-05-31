using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface ITourService
    {
        Task<TourReadDto> CreateAsync(CreateTourDto dto);
        Task<TourReadDto?> UpdateAsync(UpdateTourDto dto);
        Task<bool> DeleteAsync(int id);
        Task<TourReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<TourReadDto>> GetAllAsync();
    }
}
