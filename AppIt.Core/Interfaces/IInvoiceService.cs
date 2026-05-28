using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceReadDto> CreateAsync(CreateInvoiceDto dto);
        Task<InvoiceReadDto?> UpdateAsync(UpdateInvoiceDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<InvoiceReadDto>> GetAllAsync();
        Task<IEnumerable<InvoiceReadDto>> GetByAccountIdAsync(int accountId);
        Task<InvoiceReadDto?> GetByIdAsync(int id);
        Task<InvoiceReadDto?> GetByReservationIdAsync(int reservationId);
        Task<InvoicePaymentVerificationSummaryDto> VerifyPaymentsAsync(string granularity, DateTime? atUtc = null);
    }
}
