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
    public class ProofOfPaymentService : IProofOfPaymentService
    {
        private readonly AppItDbContext _context;

        public ProofOfPaymentService(AppItDbContext context) => _context = context;

        private static ProofOfPaymentReadDto Map(ProofOfPayment p) => new()
        {
            Id = p.Id, PaymentId = p.PaymentId, InvoiceId = p.InvoiceId,
            DocumentUrl = p.DocumentUrl, Status = p.Status, UploadedAt = p.UploadedAt,
            VerifiedAt = p.VerifiedAt, VerifiedBy = p.VerifiedBy, Notes = p.Notes
        };

        public async Task<IEnumerable<ProofOfPaymentReadDto>> GetAllAsync() =>
            await _context.ProofOfPayments.AsNoTracking().OrderByDescending(p => p.UploadedAt)
                .Select(p => new ProofOfPaymentReadDto
                {
                    Id = p.Id, PaymentId = p.PaymentId, InvoiceId = p.InvoiceId,
                    DocumentUrl = p.DocumentUrl, Status = p.Status, UploadedAt = p.UploadedAt,
                    VerifiedAt = p.VerifiedAt, VerifiedBy = p.VerifiedBy, Notes = p.Notes
                }).ToListAsync();

        public async Task<ProofOfPaymentReadDto?> GetByIdAsync(int id)
        {
            var p = await _context.ProofOfPayments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return p == null ? null : Map(p);
        }

        public async Task<IEnumerable<ProofOfPaymentReadDto>> GetByPaymentAsync(int paymentId) =>
            await _context.ProofOfPayments.AsNoTracking().Where(p => p.PaymentId == paymentId)
                .Select(p => new ProofOfPaymentReadDto
                {
                    Id = p.Id, PaymentId = p.PaymentId, InvoiceId = p.InvoiceId,
                    DocumentUrl = p.DocumentUrl, Status = p.Status, UploadedAt = p.UploadedAt,
                    VerifiedAt = p.VerifiedAt, VerifiedBy = p.VerifiedBy, Notes = p.Notes
                }).ToListAsync();

        public async Task<ProofOfPaymentReadDto> CreateAsync(CreateProofOfPaymentDto dto)
        {
            var entity = new ProofOfPayment
            {
                PaymentId = dto.PaymentId, InvoiceId = dto.InvoiceId,
                DocumentUrl = dto.DocumentUrl, Notes = dto.Notes
            };
            _context.ProofOfPayments.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<ProofOfPaymentReadDto?> UpdateAsync(UpdateProofOfPaymentDto dto)
        {
            var entity = await _context.ProofOfPayments.FindAsync(dto.Id);
            if (entity == null) return null;
            entity.PaymentId = dto.PaymentId; entity.InvoiceId = dto.InvoiceId;
            entity.DocumentUrl = dto.DocumentUrl; entity.Notes = dto.Notes; entity.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<ProofOfPaymentReadDto?> VerifyAsync(int id, string verifiedBy)
        {
            var entity = await _context.ProofOfPayments.FindAsync(id);
            if (entity == null) return null;
            entity.Status = "Verified"; entity.VerifiedAt = DateTime.UtcNow; entity.VerifiedBy = verifiedBy;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ProofOfPayments.FindAsync(id);
            if (entity == null) return false;
            _context.ProofOfPayments.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
