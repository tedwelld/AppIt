using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface ICreditNoteService
    {
        Task<IEnumerable<CreditNoteReadDto>> GetAllAsync();
        Task<CreditNoteReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<CreditNoteReadDto>> GetByReservationAsync(int reservationId);
        Task<IEnumerable<CreditNoteReadDto>> GetByInvoiceAsync(int invoiceId);
        Task<CreditNoteReadDto> CreateAsync(CreateCreditNoteDto dto);
        Task<CreditNoteReadDto?> UpdateAsync(UpdateCreditNoteDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
