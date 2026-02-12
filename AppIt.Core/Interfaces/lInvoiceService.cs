using AppIt.Core.DTOs;

namespace AppIt.Core.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceReadDto> CreateAsync(CreateInvoiceDto dto);
        Task<InvoiceReadDto?> UpdateAsync(UpdateInvoiceDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<InvoiceReadDto>> GetAllAsync();
        Task<InvoiceReadDto?> GetByIdAsync(int id);
    }
}
