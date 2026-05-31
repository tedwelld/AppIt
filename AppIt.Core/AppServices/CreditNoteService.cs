using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppIt.Core.AppServices
{
    public class CreditNoteService : ICreditNoteService
    {
        private readonly AppItDbContext _context;

        public CreditNoteService(AppItDbContext context) => _context = context;

        private static CreditNoteReadDto Map(CreditNote c) => new()
        {
            Id = c.Id, InvoiceId = c.InvoiceId, ReservationId = c.ReservationId,
            Reason = c.Reason, Amount = c.Amount, CurrencyCode = c.CurrencyCode,
            Status = c.Status, Notes = c.Notes, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
        };

        public async Task<IEnumerable<CreditNoteReadDto>> GetAllAsync() =>
            await _context.CreditNotes.AsNoTracking().OrderByDescending(c => c.CreatedAt).Select(c => new CreditNoteReadDto
            {
                Id = c.Id, InvoiceId = c.InvoiceId, ReservationId = c.ReservationId,
                Reason = c.Reason, Amount = c.Amount, CurrencyCode = c.CurrencyCode,
                Status = c.Status, Notes = c.Notes, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
            }).ToListAsync();

        public async Task<CreditNoteReadDto?> GetByIdAsync(int id)
        {
            var c = await _context.CreditNotes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? null : Map(c);
        }

        public async Task<IEnumerable<CreditNoteReadDto>> GetByReservationAsync(int reservationId) =>
            await _context.CreditNotes.AsNoTracking().Where(c => c.ReservationId == reservationId)
                .Select(c => new CreditNoteReadDto
                {
                    Id = c.Id, InvoiceId = c.InvoiceId, ReservationId = c.ReservationId,
                    Reason = c.Reason, Amount = c.Amount, CurrencyCode = c.CurrencyCode,
                    Status = c.Status, Notes = c.Notes, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
                }).ToListAsync();

        public async Task<IEnumerable<CreditNoteReadDto>> GetByInvoiceAsync(int invoiceId) =>
            await _context.CreditNotes.AsNoTracking().Where(c => c.InvoiceId == invoiceId)
                .Select(c => new CreditNoteReadDto
                {
                    Id = c.Id, InvoiceId = c.InvoiceId, ReservationId = c.ReservationId,
                    Reason = c.Reason, Amount = c.Amount, CurrencyCode = c.CurrencyCode,
                    Status = c.Status, Notes = c.Notes, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
                }).ToListAsync();

        public async Task<CreditNoteReadDto> CreateAsync(CreateCreditNoteDto dto)
        {
            var entity = new CreditNote
            {
                InvoiceId = dto.InvoiceId, ReservationId = dto.ReservationId,
                Reason = dto.Reason, Amount = dto.Amount, CurrencyCode = dto.CurrencyCode,
                Status = dto.Status, Notes = dto.Notes
            };
            _context.CreditNotes.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<CreditNoteReadDto?> UpdateAsync(UpdateCreditNoteDto dto)
        {
            var entity = await _context.CreditNotes.FindAsync(dto.Id);
            if (entity == null) return null;
            entity.InvoiceId = dto.InvoiceId; entity.ReservationId = dto.ReservationId;
            entity.Reason = dto.Reason; entity.Amount = dto.Amount;
            entity.CurrencyCode = dto.CurrencyCode; entity.Status = dto.Status;
            entity.Notes = dto.Notes; entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.CreditNotes.FindAsync(id);
            if (entity == null) return false;
            _context.CreditNotes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
