using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IRefundService
    {
        Task<IEnumerable<RefundReadDto>> GetAllAsync();
        Task<RefundReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<RefundReadDto>> GetByPaymentAsync(int paymentId);
        Task<RefundReadDto> CreateAsync(CreateRefundDto dto);
        Task<RefundReadDto?> UpdateAsync(UpdateRefundDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
