using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IVoucherService
    {
        Task<VoucherReadDto> CreateAsync(CreateVoucherDto dto);
        Task<VoucherReadDto?> UpdateAsync(UpdateVoucherDto dto);
        Task<bool> DeleteAsync(int id);
        Task<VoucherReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<VoucherReadDto>> GetAllAsync();
    }
}
