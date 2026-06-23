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

        public async Task<JournalRunResultDto> RunJournalTransactionsAsync(DateTime? processingDate = null)
        {
            var date = (processingDate ?? DateTime.UtcNow).Date;
            await EnsureDefaultAccountsAsync();

            var revenue = await _context.FinancialAccounts.FirstAsync(a => a.PastelRef == "REV");
            var receivable = await _context.FinancialAccounts.FirstAsync(a => a.PastelRef == "AR");

            var items = await _context.ReservationServiceItems
                .Include(i => i.Reservation)
                .Where(i => !i.IsPostedToJournal && i.Reservation != null && i.Reservation.Status == "Closed")
                .ToListAsync();

            var entriesCreated = 0;
            var linesCreated = 0;
            foreach (var group in items.GroupBy(i => i.ReservationId))
            {
                var total = group.Sum(i => i.TotalPrice);
                if (total <= 0) continue;

                var entry = new JournalEntry
                {
                    ReservationId = group.Key,
                    Description = $"Booking journal {date:yyyy-MM-dd}",
                    JournalType = "Reservation",
                    VoucherReference = group.Key
                };
                entry.JournalEntryLines.Add(new JournalEntryLine
                {
                    FinancialAccountId = receivable.Id, Amount = total, EntryType = "Debit"
                });
                entry.JournalEntryLines.Add(new JournalEntryLine
                {
                    FinancialAccountId = revenue.Id, Amount = total, EntryType = "Credit"
                });
                _context.JournalEntries.Add(entry);
                entriesCreated++;
                linesCreated += 2;

                foreach (var item in group) item.IsPostedToJournal = true;
                receivable.Balance += total;
                revenue.Balance += total;
            }

            await _context.SaveChangesAsync();
            return new JournalRunResultDto
            {
                EntriesCreated = entriesCreated,
                LinesCreated = linesCreated,
                ProcessingDate = date,
                Message = $"Posted {entriesCreated} journal entries."
            };
        }

        public async Task<int> DeleteExistingJournalEntriesAsync(DateTime? processingDate = null)
        {
            var date = (processingDate ?? DateTime.UtcNow).Date;
            var entries = await _context.JournalEntries
                .Where(j => j.CreatedAt.Date == date)
                .Include(j => j.JournalEntryLines)
                .ToListAsync();
            _context.JournalEntryLines.RemoveRange(entries.SelectMany(e => e.JournalEntryLines));
            _context.JournalEntries.RemoveRange(entries);
            await _context.SaveChangesAsync();
            return entries.Count;
        }

        private async Task EnsureDefaultAccountsAsync()
        {
            if (await _context.FinancialAccounts.AnyAsync()) return;
            _context.FinancialAccounts.AddRange(
                new FinancialAccount { Order = 1, PastelRef = "REV", Name = "Revenue", AccountType = "Income" },
                new FinancialAccount { Order = 2, PastelRef = "AR", Name = "Accounts Receivable", AccountType = "Assets" },
                new FinancialAccount { Order = 3, PastelRef = "CASH", Name = "Cash", AccountType = "Assets" }
            );
            await _context.SaveChangesAsync();
        }
    }
}
