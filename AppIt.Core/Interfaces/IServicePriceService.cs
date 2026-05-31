using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IServicePriceService
    {
        Task<IEnumerable<ServicePriceReadDto>> GetAllAsync();
        Task<ServicePriceReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServicePriceReadDto>> GetByServiceAsync(string serviceType, int serviceId);
        Task<ServicePriceReadDto> CreateAsync(CreateServicePriceDto dto);
        Task<ServicePriceReadDto?> UpdateAsync(UpdateServicePriceDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
