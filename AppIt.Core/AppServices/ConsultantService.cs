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
    public class ConsultantService : IConsultantService
    {
        private readonly AppItDbContext _context;

        public ConsultantService(AppItDbContext context) => _context = context;

        private static ConsultantReadDto Map(Consultant c) => new()
        {
            Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, Email = c.Email,
            Phone = c.Phone, CompanyId = c.CompanyId, CommissionRate = c.CommissionRate,
            IsActive = c.IsActive, CreatedAt = c.CreatedAt
        };

        public async Task<IEnumerable<ConsultantReadDto>> GetAllAsync() =>
            await _context.Consultants.AsNoTracking().OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
                .Select(c => new ConsultantReadDto
                {
                    Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, Email = c.Email,
                    Phone = c.Phone, CompanyId = c.CompanyId, CommissionRate = c.CommissionRate,
                    IsActive = c.IsActive, CreatedAt = c.CreatedAt
                }).ToListAsync();

        public async Task<ConsultantReadDto?> GetByIdAsync(int id)
        {
            var c = await _context.Consultants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? null : Map(c);
        }

        public async Task<ConsultantReadDto> CreateAsync(CreateConsultantDto dto)
        {
            await EnsureUniqueConsultantAsync(dto.FirstName, dto.LastName, dto.Email, null);
            var entity = new Consultant
            {
                FirstName = dto.FirstName.Trim(), LastName = dto.LastName.Trim(), Email = NormalizeOptional(dto.Email),
                Phone = dto.Phone, CompanyId = dto.CompanyId,
                CommissionRate = dto.CommissionRate, IsActive = dto.IsActive
            };
            _context.Consultants.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<ConsultantReadDto?> UpdateAsync(UpdateConsultantDto dto)
        {
            var entity = await _context.Consultants.FindAsync(dto.Id);
            if (entity == null) return null;
            await EnsureUniqueConsultantAsync(dto.FirstName, dto.LastName, dto.Email, dto.Id);
            entity.FirstName = dto.FirstName.Trim(); entity.LastName = dto.LastName.Trim();
            entity.Email = NormalizeOptional(dto.Email); entity.Phone = dto.Phone;
            entity.CompanyId = dto.CompanyId; entity.CommissionRate = dto.CommissionRate;
            entity.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Consultants.FindAsync(id);
            if (entity == null) return false;
            _context.Consultants.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task EnsureUniqueConsultantAsync(string firstName, string lastName, string? email, int? currentId)
        {
            var normalizedFirst = (firstName ?? string.Empty).Trim().ToLower();
            var normalizedLast = (lastName ?? string.Empty).Trim().ToLower();
            var normalizedEmail = NormalizeOptional(email)?.ToLower();
            if (string.IsNullOrWhiteSpace(normalizedFirst) || string.IsNullOrWhiteSpace(normalizedLast))
            {
                throw new InvalidOperationException("Consultant first name and last name are required.");
            }

            var exists = await _context.Consultants.AnyAsync(c =>
                c.Id != currentId
                && (
                    (normalizedEmail != null && c.Email != null && c.Email.ToLower() == normalizedEmail)
                    || (c.FirstName.ToLower() == normalizedFirst && c.LastName.ToLower() == normalizedLast)
                ));
            if (exists)
            {
                throw new InvalidOperationException("This consultant already exists.");
            }
        }

        private static string? NormalizeOptional(string? value)
        {
            var normalized = value?.Trim();
            return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
        }
    }
}
