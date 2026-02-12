using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IAccommodationService
    {
        Task<AccommodationReadDto> CreateAsync(CreateAccommodationDto dto);
        Task<AccommodationReadDto?> UpdateAsync(UpdateAccommodationDto dto);
        Task<bool> DeleteAsync(int id);
        Task<AccommodationReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<AccommodationReadDto>> GetAllAsync();
    }
}
