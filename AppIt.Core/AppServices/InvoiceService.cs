using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.AppServices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppItDbContext _context;

        public InvoiceService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceReadDto> CreateAsync(CreateInvoiceDto dto)
        {
            var invoice = new Invoice
            {
                ReservationId = dto.ReservationId,
                TotalAmount = dto.TotalAmount,
                CurrencyCode = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                IsPaid = dto.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase),
                IssuedDate = DateTime.UtcNow
            };

            _context.Add(invoice);
            await _context.SaveChangesAsync();

            return ToReadDto(invoice);
        }

        public async Task<InvoiceReadDto?> UpdateAsync(UpdateInvoiceDto dto)
        {
            var invoice = await _context.Set<Invoice>().FindAsync(dto.Id);
            if (invoice == null) return null;

            invoice.ReservationId = dto.ReservationId;
            invoice.TotalAmount = dto.TotalAmount;
            invoice.CurrencyCode = string.IsNullOrWhiteSpace(dto.Currency) ? invoice.CurrencyCode : dto.Currency;
            invoice.Status = string.IsNullOrWhiteSpace(dto.Status) ? invoice.Status : dto.Status;
            invoice.IsPaid = invoice.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase);

            await _context.SaveChangesAsync();
            return ToReadDto(invoice);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoice = await _context.Set<Invoice>().FindAsync(id);
            if (invoice == null) return false;

            _context.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InvoiceReadDto>> GetAllAsync()
        {
            return await _context.Set<Invoice>()
                .AsNoTracking()
                .Select(i => ToReadDto(i))
                .ToListAsync();
        }

        public async Task<InvoiceReadDto?> GetByIdAsync(int id)
        {
            var invoice = await _context.Set<Invoice>()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
            return invoice == null ? null : ToReadDto(invoice);
        }

        private static InvoiceReadDto ToReadDto(Invoice invoice)
        {
            return new InvoiceReadDto
            {
                Id = invoice.Id,
                ReservationId = invoice.ReservationId,
                TotalAmount = invoice.TotalAmount,
                Currency = invoice.CurrencyCode,
                Status = invoice.Status,
                IssuedAt = invoice.IssuedDate
            };
        }
    }
}
