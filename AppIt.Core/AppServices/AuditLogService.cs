using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.AppServices
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppItDbContext _context;

        public AuditLogService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLogReadDto>> GetAllAsync()
        {
            return await _context.Set<AuditLog>()
                .AsNoTracking()
                .OrderByDescending(a => a.PerformedAt)
                .ThenByDescending(a => a.Id)
                .Select(a => new AuditLogReadDto(
                    a.Id,
                    a.EntityName ?? string.Empty,
                    a.EntityId ?? string.Empty,
                    a.Action ?? string.Empty,
                    a.Changes,
                    a.PerformedBy,
                    a.PerformedAt))
                .ToListAsync();
        }

        public async Task<AuditLogReadDto?> GetByIdAsync(int id)
        {
            var a = await _context.Set<AuditLog>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (a == null) return null;
            return new AuditLogReadDto(
                a.Id,
                a.EntityName ?? string.Empty,
                a.EntityId ?? string.Empty,
                a.Action ?? string.Empty,
                a.Changes,
                a.PerformedBy,
                a.PerformedAt);
        }
    }

}
