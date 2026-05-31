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
    public class CommissionService : ICommissionService
    {
        private readonly AppItDbContext _context;

        public CommissionService(AppItDbContext context) => _context = context;

        private static CommissionReadDto Map(Commission c) => new()
        {
            Id = c.Id, ReservationId = c.ReservationId, ConsultantId = c.ConsultantId,
            Percentage = c.Percentage, Amount = c.Amount, CurrencyCode = c.CurrencyCode,
            Status = c.Status, PaidAt = c.PaidAt, Notes = c.Notes,
            CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
        };

        public async Task<IEnumerable<CommissionReadDto>> GetAllAsync() =>
            await _context.Commissions.AsNoTracking().OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommissionReadDto
                {
                    Id = c.Id, ReservationId = c.ReservationId, ConsultantId = c.ConsultantId,
                    Percentage = c.Percentage, Amount = c.Amount, CurrencyCode = c.CurrencyCode,
                    Status = c.Status, PaidAt = c.PaidAt, Notes = c.Notes,
                    CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
                }).ToListAsync();

        public async Task<CommissionReadDto?> GetByIdAsync(int id)
        {
            var c = await _context.Commissions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? null : Map(c);
        }

        public async Task<IEnumerable<CommissionReadDto>> GetByReservationAsync(int reservationId) =>
            await _context.Commissions.AsNoTracking().Where(c => c.ReservationId == reservationId)
                .Select(c => new CommissionReadDto
                {
                    Id = c.Id, ReservationId = c.ReservationId, ConsultantId = c.ConsultantId,
                    Percentage = c.Percentage, Amount = c.Amount, CurrencyCode = c.CurrencyCode,
                    Status = c.Status, PaidAt = c.PaidAt, Notes = c.Notes,
                    CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
                }).ToListAsync();

        public async Task<CommissionReadDto> CreateAsync(CreateCommissionDto dto)
        {
            var entity = new Commission
            {
                ReservationId = dto.ReservationId, ConsultantId = dto.ConsultantId,
                Percentage = dto.Percentage, Amount = dto.Amount,
                CurrencyCode = dto.CurrencyCode, Status = dto.Status, Notes = dto.Notes
            };
            _context.Commissions.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<CommissionReadDto?> UpdateAsync(UpdateCommissionDto dto)
        {
            var entity = await _context.Commissions.FindAsync(dto.Id);
            if (entity == null) return null;
            entity.ReservationId = dto.ReservationId; entity.ConsultantId = dto.ConsultantId;
            entity.Percentage = dto.Percentage; entity.Amount = dto.Amount;
            entity.CurrencyCode = dto.CurrencyCode; entity.Status = dto.Status;
            entity.PaidAt = dto.PaidAt; entity.Notes = dto.Notes;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Commissions.FindAsync(id);
            if (entity == null) return false;
            _context.Commissions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
