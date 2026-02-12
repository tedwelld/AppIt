using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface ISupportMessageService
    {
        Task<SupportMessageReadDto> CreateAsync(CreateSupportMessageDto dto);
        Task<SupportMessageReadDto?> UpdateAsync(UpdateSupportMessageDto dto);
        Task<bool> DeleteAsync(int id);
        Task<SupportMessageReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<SupportMessageReadDto>> GetAllAsync();
    }
}
