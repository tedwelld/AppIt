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
    public class DayEndService : IDayEndService
    {
        private readonly AppItDbContext _context;

        public DayEndService(AppItDbContext context) => _context = context;

        private static DayEndReadDto Map(DayEnd d) => new()
        {
            Id = d.Id, AuditDate = d.AuditDate, OpenedBy = d.OpenedBy, ClosedBy = d.ClosedBy,
            OpenedAt = d.OpenedAt, ClosedAt = d.ClosedAt, TotalRevenue = d.TotalRevenue,
            Status = d.Status, Notes = d.Notes
        };

        public async Task<IEnumerable<DayEndReadDto>> GetAllAsync() =>
            await _context.DayEnds.AsNoTracking().OrderByDescending(d => d.AuditDate)
                .Select(d => new DayEndReadDto
                {
                    Id = d.Id, AuditDate = d.AuditDate, OpenedBy = d.OpenedBy, ClosedBy = d.ClosedBy,
                    OpenedAt = d.OpenedAt, ClosedAt = d.ClosedAt, TotalRevenue = d.TotalRevenue,
                    Status = d.Status, Notes = d.Notes
                }).ToListAsync();

        public async Task<DayEndReadDto?> GetByIdAsync(int id)
        {
            var d = await _context.DayEnds.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return d == null ? null : Map(d);
        }

        public async Task<DayEndReadDto?> GetTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            var d = await _context.DayEnds.AsNoTracking().FirstOrDefaultAsync(x => x.AuditDate == today);
            return d == null ? null : Map(d);
        }

        public async Task<DayEndReadDto> OpenAsync(OpenDayEndDto dto, string openedBy)
        {
            var existing = await _context.DayEnds.FirstOrDefaultAsync(d => d.AuditDate == dto.AuditDate.Date);
            if (existing != null)
                throw new InvalidOperationException($"Day-end for {dto.AuditDate:yyyy-MM-dd} already exists.");

            var totalRevenue = await _context.Payments.AsNoTracking()
                .Where(p => p.ProcessedAt != null && p.ProcessedAt.Value.Date == dto.AuditDate.Date)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var entity = new DayEnd
            {
                AuditDate = dto.AuditDate.Date, OpenedBy = openedBy,
                TotalRevenue = totalRevenue, Status = "Open", Notes = dto.Notes
            };
            _context.DayEnds.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<DayEndReadDto?> CloseAsync(CloseDayEndDto dto, string closedBy)
        {
            var entity = await _context.DayEnds.FindAsync(dto.Id);
            if (entity == null) return null;
            if (entity.Status == "Closed")
                throw new InvalidOperationException("Day-end is already closed.");

            entity.Status = "Closed"; entity.ClosedBy = closedBy;
            entity.ClosedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.Notes)) entity.Notes = dto.Notes;
            await _context.SaveChangesAsync();
            return Map(entity);
        }
    }
}
