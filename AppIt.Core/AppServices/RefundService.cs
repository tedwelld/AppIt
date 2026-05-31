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
    public class RefundService : IRefundService
    {
        private readonly AppItDbContext _context;

        public RefundService(AppItDbContext context) => _context = context;

        private static RefundReadDto Map(Refund r) => new()
        {
            Id = r.Id, PaymentId = r.PaymentId, InvoiceId = r.InvoiceId,
            Reason = r.Reason, Amount = r.Amount, CurrencyCode = r.CurrencyCode,
            Status = r.Status, ProcessedAt = r.ProcessedAt, ProcessedBy = r.ProcessedBy,
            CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
        };

        public async Task<IEnumerable<RefundReadDto>> GetAllAsync() =>
            await _context.Refunds.AsNoTracking().OrderByDescending(r => r.CreatedAt)
                .Select(r => new RefundReadDto
                {
                    Id = r.Id, PaymentId = r.PaymentId, InvoiceId = r.InvoiceId,
                    Reason = r.Reason, Amount = r.Amount, CurrencyCode = r.CurrencyCode,
                    Status = r.Status, ProcessedAt = r.ProcessedAt, ProcessedBy = r.ProcessedBy,
                    CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
                }).ToListAsync();

        public async Task<RefundReadDto?> GetByIdAsync(int id)
        {
            var r = await _context.Refunds.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return r == null ? null : Map(r);
        }

        public async Task<IEnumerable<RefundReadDto>> GetByPaymentAsync(int paymentId) =>
            await _context.Refunds.AsNoTracking().Where(r => r.PaymentId == paymentId)
                .Select(r => new RefundReadDto
                {
                    Id = r.Id, PaymentId = r.PaymentId, InvoiceId = r.InvoiceId,
                    Reason = r.Reason, Amount = r.Amount, CurrencyCode = r.CurrencyCode,
                    Status = r.Status, ProcessedAt = r.ProcessedAt, ProcessedBy = r.ProcessedBy,
                    CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
                }).ToListAsync();

        public async Task<RefundReadDto> CreateAsync(CreateRefundDto dto)
        {
            var entity = new Refund
            {
                PaymentId = dto.PaymentId, InvoiceId = dto.InvoiceId,
                Reason = dto.Reason, Amount = dto.Amount, CurrencyCode = dto.CurrencyCode, Status = dto.Status
            };
            _context.Refunds.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<RefundReadDto?> UpdateAsync(UpdateRefundDto dto)
        {
            var entity = await _context.Refunds.FindAsync(dto.Id);
            if (entity == null) return null;
            entity.PaymentId = dto.PaymentId; entity.InvoiceId = dto.InvoiceId;
            entity.Reason = dto.Reason; entity.Amount = dto.Amount;
            entity.CurrencyCode = dto.CurrencyCode; entity.Status = dto.Status;
            entity.ProcessedAt = dto.ProcessedAt; entity.ProcessedBy = dto.ProcessedBy;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Refunds.FindAsync(id);
            if (entity == null) return false;
            _context.Refunds.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
