using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface ITransferService
    {
        Task<TransferReadDto> CreateAsync(CreateTransferDto dto);
        Task<TransferReadDto?> UpdateAsync(UpdateTransferDto dto);
        Task<bool> DeleteAsync(int id);
        Task<TransferReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<TransferReadDto>> GetAllAsync();
    }
}
