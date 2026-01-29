using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.Entities;
using AppIt.Data.Entities.AppIt.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using System;

namespace AppIt.Core.Services
{
    public class ReportSnapshotService : IReportSnapshotService
    {
        private readonly AppItDbContext _context;

        public ReportSnapshotService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReportSnapshotDto>> GetByReportKeyAsync(string reportKey)
        {
            return await _context.ReportSnapshots
                .AsNoTracking()
                .Where(r => r.ReportKey == reportKey)
                .OrderByDescending(r => r.SnapshotDate)
                .Select(r => new ReportSnapshotDto
                {
                    Id = r.Id,
                    ReportKey = r.ReportKey,
                    Title = r.Title,
                    SnapshotDate = r.SnapshotDate
                })
                .ToListAsync();
        }

        public async Task<ReportSnapshotDetailDto?> GetByIdAsync(int id)
        {
            return await _context.ReportSnapshots
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new ReportSnapshotDetailDto
                {
                    Id = r.Id,
                    ReportKey = r.ReportKey,
                    Title = r.Title,
                    DataJson = r.DataJson,
                    SnapshotDate = r.SnapshotDate,
                    GeneratedByUserId = r.GeneratedByUserId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(CreateReportSnapshotDto dto)
        {
            var snapshot = new ReportSnapshot
            {
                ReportKey = dto.ReportKey,
                Title = dto.Title,
                DataJson = dto.DataJson,
                SnapshotDate = dto.SnapshotDate,
                GeneratedByUserId = dto.GeneratedByUserId
            };

            _context.ReportSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();

            return snapshot.Id;
        }
    }
}
