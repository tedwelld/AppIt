using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentReadDto> CreateAsync(CreatePaymentDto dto);
        Task<ProcessPaymentResultDto> ProcessAsync(ProcessPaymentDto dto);
        Task<PaymentReadDto?> UpdateAsync(UpdatePaymentDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PaymentReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<PaymentReadDto>> GetAllAsync();
        Task<IEnumerable<PaymentReadDto>> GetByAccountIdAsync(int accountId);
        Task<int> DeleteExpiredPendingPaymentsAsync(TimeSpan? maxAge = null);
    }
}
