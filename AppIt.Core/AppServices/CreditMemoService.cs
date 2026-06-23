using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class CreditMemoService : ICreditMemoService
    {
        private readonly AppItDbContext _context;

        public CreditMemoService(AppItDbContext context) => _context = context;

        public async Task<IEnumerable<CreditMemoReadDto>> GetAllAsync() =>
            await _context.CreditMemos.AsNoTracking().OrderByDescending(c => c.CreatedAt)
                .Select(c => new CreditMemoReadDto
                {
                    Id = c.Id, ReservationId = c.ReservationId, TotalAmount = c.TotalAmount,
                    CurrencyCode = c.CurrencyCode, Notes = c.Notes
                }).ToListAsync();

        public async Task<CreditMemoReadDto> CreateAsync(CreateCreditMemoDto dto)
        {
            var entity = new CreditMemo
            {
                ReservationId = dto.ReservationId, CreditNoteId = dto.CreditNoteId,
                InvoiceId = dto.InvoiceId, TotalAmount = dto.TotalAmount,
                CurrencyCode = dto.CurrencyCode, Notes = dto.Notes
            };
            _context.CreditMemos.Add(entity);
            await _context.SaveChangesAsync();
            return new CreditMemoReadDto
            {
                Id = entity.Id, ReservationId = entity.ReservationId,
                TotalAmount = entity.TotalAmount, CurrencyCode = entity.CurrencyCode, Notes = entity.Notes
            };
        }
    }
}
